using System.Reflection;
using Microsoft.Extensions.Logging;
using SlowLang.Engine.Statements.StatementRegistrations;
using SlowLang.Engine.Tokens;
using SlowLang.Engine.Values;

namespace SlowLang.Engine.Statements;

/// <summary>
/// Represents a single statement in a script
/// </summary>
public abstract class Statement
{
    protected static readonly ILogger Logger =
        LoggingManager.LoggerFactory.CreateLogger("SlowLang.Statements");


    private static bool isInitialized;

    public int LineNumber { get; private set; }

    /// <summary>
    /// If this returns true, the Statement will have to cut itself from the token list
    /// </summary>
    protected virtual bool CutTokensManually() => false;

    public virtual Value Execute() => SlowVoid.I;

    protected virtual bool OnParse(ref TokenList list)
    {
        return true;
    }

    private static readonly List<StatementRegistration> Registrations = new();

    public static void Register(StatementRegistration registration)
    {
        //Check if the registration is valid
        if (registration.Statement.BaseType == typeof(Statement))
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
    /// <returns>A fully configured Statement</returns>
    public static Statement? Parse(ref TokenList list, bool throwError = true)
    {
        //If the Parser wasn't initialized yet, do it now
        if (!isInitialized)
            Initialize();

        Statement? statement = null;
        foreach (StatementRegistration registration in Registrations)
        {
            statement = ParseStatementFromRegistration(registration, list);

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

    private static Statement? ParseStatementFromRegistration(StatementRegistration registration, TokenList tokenList)
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


        //Invoke its OnParse() callback
        statement.OnParse(ref tokenList);

        //Remove the tokens that match from the token list
        if (!statement.CutTokensManually())
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
            statement.OnParse(ref list, baseStatement);

            //Remove the tokens that match from the token list
            if (!statement.CutTokensManually())
                list.List.RemoveRange(0, registration.Match.Length);

            return ParseStatementExtension(statement, ref list) ?? statement;

            next: ;
        }


        return null;
    }

    public static Statement[] ParseMultiple(TokenList list)
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
        foreach (Type type in ParsingUtility.GetAllInheritors(typeof(Statement)))
        {
            //Ignore abstract Statement inheritors
            if (type.IsAbstract)
                continue;

            //Get a static method called OnInitialize inside of them
            MethodInfo? initMethod = type.GetMethod("OnInitialize");

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