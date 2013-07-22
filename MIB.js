//pork fork from https://github.com/lextm/sharpsnmplib/blob/master/Archive/Mib/
//
//
//
var Definitions = new Symbol("DEFINITIONS");
var  Begin = new Symbol("BEGIN");        
var  Object = new Symbol("OBJECT");        
var  Identifier = new Symbol("IDENTIFIER");
var  Assign = new Symbol("::=");
var  OpenBracket = new Symbol("{");
var  CloseBracket = new Symbol("}");
var  Imports = new Symbol("IMPORTS");
var  Semicolon = new Symbol(";");
var  From = new Symbol("FROM");
var  ModuleIdentity = new Symbol("MODULE-IDENTITY");
var  ObjectType = new Symbol("OBJECT-TYPE");
var  ObjectGroup = new Symbol("OBJECT-GROUP");
var  NotificationGroup = new Symbol("NOTIFICATION-GROUP");
var  ModuleCompliance = new Symbol("MODULE-COMPLIANCE");
var  Sequence = new Symbol("SEQUENCE");
var  NotificationType = new Symbol("NOTIFICATION-TYPE");
var  EOL = new Symbol(Environment.NewLine);
var  ObjectIdentity = new Symbol("OBJECT-IDENTITY");
var  End = new Symbol("END");
var  Macro = new Symbol("MACRO");
var  Choice = new Symbol("CHOICE");
var  TrapType = new Symbol("TRAP-TYPE");
var  AgentCapabilities = new Symbol("AGENT-CAPABILITIES");
var  Comma = new Symbol(",");
var  TextualConvention = new Symbol("TEXTUAL-CONVENTION");
var  Syntax = new Symbol("SYNTAX");
var  Integer = new Symbol("INTEGER");
var  Bits = new Symbol("BITS");
var  Octet = new Symbol("OCTET");
var  String = new Symbol("STRING");
var  OpenParentheses = new Symbol("(");
var  CloseParentheses = new Symbol(")");
var  Exports = new Symbol("EXPORTS");
var  DisplayHint = new Symbol("DISPLAY-HINT");
var  Status = new Symbol("STATUS");
var  Description = new Symbol("DESCRIPTION");
var  Reference = new Symbol("REFERENCE");
var  DoubleDot = new Symbol("..");
var  Integer32 = new Symbol("Integer32");
var  IpAddress = new Symbol("IpAddress");
var  Counter32 = new Symbol("Counter32");
var  TimeTicks = new Symbol("TimeTicks");
var  Opaque = new Symbol("Opaque");
var  Counter64 = new Symbol("Counter64");
var  Unsigned32 = new Symbol("Unsigned32");
var  Gauge32 = new Symbol("Gauge32");
var  Size = new Symbol("SIZE");
var  Units = new Symbol("UNITS");
var  MaxAccess = new Symbol("MAX-ACCESS");
var  Access = new Symbol("ACCESS");
var  Index = new Symbol("INDEX");
var  Augments = new Symbol("AUGMENTS");
var  DefVal = new Symbol("DEFVAL");
var  Of = new Symbol("OF");

function Symbol(name){
    return(name);
}
/// <summary>
/// Parses a list of <see cref="char"/> to <see cref="Symbol"/>.
/// </summary>
/// <param name="file">File</param>
/// <param name="current">Current <see cref="char"/></param>
/// <param name="row">Row number</param>
/// <param name="column">Column number</param>
/// <returns><code>true</code> if no need to process this line. Otherwise, <code>false</code> is returned.</returns>
function Parse(file, current, row, column)//bool
{
            switch (current){
                case '\n':
                    if (!_stringSection)
                    {
                        _buffer.Fill(_symbols, file, row, column);
                        _symbols.Add(CreateSpecialSymbol(file, current, row, column));
                        return false;
                    }

                    break;
                case '{':
                case '}':
                case '(':
                case ')':
                case '[':
                case ']':
                case ';':
                case ',':
                case '|':
                    if (_commentSection){
                    break;
                    }

                    if (!_stringSection){
                    _buffer.Fill(_symbols, file, row, column);
                    _symbols.Add(CreateSpecialSymbol(file, current, row, column));
                    return false;
                    }

                    break;
                case '"':
                    if (_commentSection){
                        break;
                    }
                    _stringSection = !_stringSection;
                    break;
                case '-':
                    if (_stringSection){
                        break;
                    }

                    if (!_singleDashFound){
                        _singleDashFound = true;
                        break;
                    }

                    _singleDashFound = false;
                    _commentSection = !_commentSection;
                    break;
                case '\r':
                    return false;
                default:
                    if (current == 0x1A){
                        // IMPORTANT: ignore invisible characters such as SUB.
                        return false;
                    }

                    _singleDashFound = false;
                    if (Char.IsWhiteSpace(current) && !_assignSection && !_stringSection && !_commentSection){
                        _buffer.Fill(_symbols, file, row, column);
                        return false;
                    }

                    if (_commentSection){
                        // TODO: ignore everything here in comment
                        break;
                    }

                    if (_assignAhead){
                        _assignAhead = false;
                        _buffer.Fill(_symbols, file, row, column);
                        break;
                    }

                    if (_dotSection && current != '.'){
                        _buffer.Fill(_symbols, file, row, column);
                        _dotSection = false;
                    }

                    if (current == '.' && !_stringSection){
                        if (!_dotSection)
                        {
                            _buffer.Fill(_symbols, file, row, column);
                            _dotSection = true;
                        }
                    }
                    
                    if (current == ':' && !_stringSection){    
                        if (!_assignSection)
                        {
                            _buffer.Fill(_symbols, file, row, column);
                        }
                        
                        _assignSection = true;
                    }
                    
                    if (current == '=' && !_stringSection){
                        _assignSection = false; 
                        _assignAhead = true;
                    }

                    break;
                    }

                    _buffer.Append(current);
                    return false;
}
