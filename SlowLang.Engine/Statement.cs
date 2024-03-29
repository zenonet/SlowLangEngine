﻿using System.Reflection;
using Microsoft.Extensions.Logging;
using SlowLang.Engine.Initialization;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace SlowLang.Engine.Statements;

/// <summary>
/// Represents a single statement in a script
/// </summary>
public abstract class Statement
{
    private static readonly Lazy<ILogger> LazyLogger = new(() => LoggingManager.LoggerFactory.CreateLogger("SlowLang.Statements"));

    protected static ILogger Logger => LazyLogger.Value;


    private static bool isInitialized;

    public int LineNumber { get; private set; }

    /// <summary>
    /// If this returns true, the Statement will have to cut itself from the token list
    /// </summary>
    protected virtual bool CutTokensManually() => false;

    public virtual Value Execute() => SlowVoid.I;

    public virtual bool OnParse(ref TokenList list)
    {
        return true;
    }

    private static readonly List<StatementRegistration> Registrations = new();

    public static void Register(StatementRegistration registration)
    {
        //Check if the registration is valid
        if (registration.Statement.IsAssignableTo(typeof(Statement)))
        {
            Registrations.Add(registration);
            return;
        }

        Logger.LogWarning(
            "A StatementRegistration exists, " +
            "which doesn't refer to a subclass of Statement"
        );
    }

    private static readonly List<StatementExtensionRegistration> ExtensionRegistrations = new();

    public static void RegisterExtension(StatementExtensionRegistration registration)
    {
        //Check if the registration is valid
        if (registration.ExtensionStatement.IsAssignableTo(typeof(StatementExtension)))
        {
            ExtensionRegistrations.Add(registration);
            return;
        }

        Logger.LogWarning("A StatementExtensionRegistration exists, which doesn't refer to a subclass of StatementExtension");
    }

    /// <summary>
    /// Parses a single statement
    /// </summary>
    /// <param name="list">A TokenList to parse from</param>
    /// <param name="throwError">If an error should get thrown if no Statement could be parsed</param>
    /// <param name="onStatementInstantiated">An event, that is called when the Statement instance is created but the OnParse method isn't called yet</param>
    /// <returns>A fully configured Statement</returns>
    public static Statement? Parse(ref TokenList list, bool throwError = true, Action<Statement>? onStatementInstantiated = null)
    {
        //If the Parser wasn't initialized yet, do it now
        if (!isInitialized)
            Initialize();

        Statement? statement = null;
        foreach (StatementRegistration registration in Registrations)
        {
            statement = ParseStatementFromRegistration(registration, ref list, onStatementInstantiated);

            if (statement != null)
                break;
        }

        if (statement != null)
            return statement;

        if (throwError)
        {
            LoggingManager.LogError($"Unexpected string '{list.Peek().RawContent}'", list.Peek().LineNumber);
        }

        return null;
    }

    private static Statement? ParseStatementFromRegistration(StatementRegistration registration, ref TokenList tokenList, Action<Statement>? onStatementInstantiated = null)
    {
        //Check if the matching sequence in the current registration would fit int o list.List
        if (registration.Match.Length > tokenList.List.Count)
            return null;

        //Iterate through all elements and check if the TokenType matches
        for (int i = 0; i < registration.Match.Length; i++)
        {
            //If it doesn't match:
            if (tokenList.List[i].Type != registration.Match[i])
                return null;
        }

        //If the StatementRegistration has a customParser defined:
        if (registration.CustomParser != null)
        {
            //Execute the custom Parser
            bool result = registration.CustomParser.Invoke(tokenList);
            if (!result) //And if it couldn't parse the TokenList, jump over the parsing stuff and continue with the next StatementRegistration
                return null;
        }

        //Instantiate the matching subclass
        Statement statement = (Activator.CreateInstance(registration.Statement) as Statement)!;

        //Set the line number
        statement.LineNumber = tokenList.List[0].LineNumber;

        onStatementInstantiated?.Invoke(statement);

        TokenList statementSideTokenList = tokenList.Clone();

        //Invoke its OnParse() callback
        if (!statement.OnParse(ref statementSideTokenList))
        {
            Logger.LogError($"Parser of {registration.Statement.Name} failed");
            return null;
        }

        if (statement.CutTokensManually())
            //Make the statements TokenList the new TokenList 
            tokenList = statementSideTokenList;
        else
            //Remove the tokens that match from the token list
            tokenList.List.RemoveRange(0, registration.Match.Length);

        Statement? extension = ParseStatementExtension(statement, ref tokenList);

        return extension ?? statement;
    }

    private static Statement? ParseStatementExtension(Statement baseStatement, ref TokenList list)
    {
        foreach (StatementExtensionRegistration registration in ExtensionRegistrations)
        {
            if (!registration.BaseStatement.IsInstanceOfType(baseStatement))
                continue;

            if (registration.Match.Length > list.List.Count)
                continue;

            //Iterate through all elements and check if the TokenType matches
            for (int i = 0; i < registration.Match.Length; i++)
            {
                //If it doesn't match:
                if (list.List[i].Type != registration.Match[i])
                    goto next; //Jump over all of the parsing stuff and continue with the next StatementRegistration
            }

            //If the StatementRegistration has a customParser defined:
            if (registration.CustomParser != null)
            {
                //Execute the custom Parser
                bool result = registration.CustomParser.Invoke(list);
                if (!result) //And if it couldn't parse the TokenList, jump over the parsing stuff and continue with the next StatementRegistration
                    goto next;
            }

            //Instantiate the matching subclass
            StatementExtension statement = (Activator.CreateInstance(registration.ExtensionStatement) as StatementExtension)!;

            //Set the line number
            statement.LineNumber = list.List[0].LineNumber;

            //Invoke its OnParse() callback
            if (!statement.OnParse(ref list, baseStatement))
            {
                Logger.LogError($"Parser of {registration.ExtensionStatement.Name} failed");
                return null;
            }

            //Remove the tokens that match from the token list
            if (!statement.CutTokensManually())
                list.List.RemoveRange(0, registration.Match.Length);

            return ParseStatementExtension(statement, ref list) ?? statement;

            next: ;
        }

        return null;
    }

    public static Statement[] ParseMultiple(ref TokenList list)
    {
        //If the Parser wasn't initialized yet, do it now
        if (!isInitialized)
            Initialize();

        List<Statement> statements = new();
        while (list.List.Count > 0)
        {
            statements.Add(Parse(ref list)!);
            list.TrimStart(TokenType.Semicolon);
        }

        return statements.ToArray();
    }

    /// <summary>
    /// Calls the static OnInitialize() method on every inheritor of Statement
    /// </summary>
    private static void Initialize()
    {
        //Iterate through all types which inherit from Statement
        foreach (Type type in ParsingUtility.GetAllInheritors(typeof(IInitializable)))
        {
            //Ignore abstract Statement inheritors
            if (type.IsAbstract)
                continue;

            //Get a static method called OnInitialize inside of them
            MethodInfo? initMethod = type.GetMethod("Initialize");

            //If it exists, call it
            if (initMethod is null)
                Logger.LogWarning("Statement subclass " + type.Name + " doesn't have an Initialize method");
            else
                initMethod.Invoke(null, null);
        }

        SortRegistrations();

        isInitialized = true;
    }

    private static void SortRegistrations()
    {
        Registrations.Sort((x, y) =>
        {
            int i = y.Match.Length - x.Match.Length; //Sorts by match length

            i += y.AdditionalPriority - x.AdditionalPriority;

            if (i == 0) //If the match length is the same
            {
                //decide by which one has a custom parser
                i =
                    (y.CustomParser != null ? 1 : 0) -
                    (x.CustomParser != null ? 1 : 0);
            }

            return i;
        });
    }

    public override string ToString()
    {
        return this.GetType().Name;
    }
}