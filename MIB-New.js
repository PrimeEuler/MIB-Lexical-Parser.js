
var fs = require('fs');


var Char = (function () {
    var _this = {
        IsControl: function (c) {
            var cc = c.charCodeAt(0);
            if ((cc >= 00 && cc <= 0x1F) || (cc == 0x7F)) {
                return true;
            }
            return false;
        },
        IsNoWSControl: function (c) {
            var cc = c.charCodeAt(0);
            if ((cc >= 00 && cc <= 0x1F) || (cc == 0x7F)) {
                if (cc >= 0x09 && cc <= 0x0D) return false;
                return true;
            }
            return false;
        },
        IsPunctuation: function (c) {
            var cc = c.charCodeAt(0);
            if ((cc >= 20 && cc <= 0x2F) || (cc >= 0x3A && cc <= 0x40) || (cc >= 0x5B && cc <= 0x60) || (cc >= 0x7B && cc <= 0x7E)) {
                return true;
            }
            return false;
        },
        IsLetterOrDigit: function (c) {
            var cc = c.charCodeAt(0);
            if ((cc >= 0x30 && cc <= 0x39) || (cc >= 0x41 && cc <= 0x5A) || (cc >= 0x61 && cc <= 0x7A)) {
                return true;
            }
            return false;
        },
        IsDigit: function (c) {
            var cc = c.charCodeAt(0);
            if (cc >= 0x30 && cc <= 0x39) {
                return true;
            }
            return false;
        },
        IsLetter: function (c) {
            var cc = c.charCodeAt(0);
            if ((cc >= 0x41 && cc <= 0x5A) || (cc >= 0x61 && cc <= 0x7A)) {
                return true;
            }
            return false;
        },
        IsWhiteSpace: function (c) {
            if (c == null) { c = ''; }
            var cc = c.charCodeAt(0);
            if ((cc >= 0x0009 && cc <= 0x000D) || (cc == 0x0020) || (cc == 0x0085) || (cc == 0x00A0) || (cc == 0x1680) || (cc == 0x180E) || (cc >= 0x2000 && cc <= 0x200A) || (cc == 0x2028) || (cc == 0x2029) || (cc == 0x202F) || (cc == 0x205F) || (cc == 0x3000)) {
                return true;
            }
            return false;
        },
        IsUnicode: function (c) {
            var cc = c.charCodeAt(0);
            if ((cc == 0x0009) || (cc == 0x000A) || (cc == 0x000D) || (cc >= 0x0020 && cc <= 0xD7FF) || (cc >= 0xE000 && cc <= 0xFFFD) || (cc >= 0x10000 && cc <= 0x10FFFF)) {
                return true;
            }
            return false;
        }
    };
    return _this;
})();

/**
* CharBuffer.cs 
*/
var CharBuffer = function () {
    var _this = {
        _builder: "",
        Append: function (current) {
            this._builder += current;
            
        },
        Fill: function (symbols, file, row, column) {
            if (this._builder.Length == 0) {
                return;
            }

            var content = this._builder.toString();
            this._builder = "";
            this._builder.length = 0;
            symbols.Add(new Symbol(file, content, row, column));
        }
    }
    return _this;
};

/**
* Symbol.cs 
*/
function Symbol (file, text, row, column) {

    this.file = file;
    this.text = text;
    this.row = row;
    this.column = column;
};
Symbol.prototype.Expect = function (expected) {
    Assert(this == expected, expected + " expected");
}
Symbol.prototype.Assert = function (condition, message) {
    if (condition) {
        return;
    }
}
Symbol.prototype.ToString = function () {
    return this.text
}
Symbol.prototype.IsComment = function (symbol) {
    return symbol != null && symbol.ToString().startsWith("--");
}

Symbol.Definitions = new Symbol("","DEFINITIONS",-1,-1);
Symbol.Definitions = new Symbol("", "DEFINITIONS", -1, -1);
Symbol.Begin = new Symbol("", "BEGIN", -1, -1);
Symbol.Object = new Symbol("", "OBJECT", -1, -1);
Symbol.Identifier = new Symbol("", "IDENTIFIER", -1, -1);
Symbol.Assign = new Symbol("", "::=", -1, -1);
Symbol.OpenBracket = new Symbol("", "{", -1, -1);
Symbol.CloseBracket = new Symbol("", "}", -1, -1);
Symbol.Imports = new Symbol("", "IMPORTS", -1, -1);
Symbol.Semicolon = new Symbol("", ";", -1, -1);
Symbol.From = new Symbol("", "FROM", -1, -1);
Symbol.ModuleIdentity = new Symbol("", "MODULE-IDENTITY", -1, -1);
Symbol.ObjectType = new Symbol("", "OBJECT-TYPE", -1, -1);
Symbol.ObjectGroup = new Symbol("", "OBJECT-GROUP", -1, -1);
Symbol.NotificationGroup = new Symbol("", "NOTIFICATION-GROUP", -1, -1);
Symbol.ModuleCompliance = new Symbol("", "MODULE-COMPLIANCE", -1, -1);
Symbol.Sequence = new Symbol("", "SEQUENCE", -1, -1);
Symbol.NotificationType = new Symbol("", "NOTIFICATION-TYPE", -1, -1);
Symbol.EOL = new Symbol("", "\n", -1, -1);
Symbol.ObjectIdentity = new Symbol("", "OBJECT-IDENTITY", -1, -1);
Symbol.End = new Symbol("", "END", -1, -1);
Symbol.Macro = new Symbol("", "MACRO", -1, -1);
Symbol.Choice = new Symbol("", "CHOICE", -1, -1);
Symbol.TrapType = new Symbol("", "TRAP-TYPE", -1, -1);
Symbol.AgentCapabilities = new Symbol("", "AGENT-CAPABILITIES", -1, -1);
Symbol.Comma = new Symbol("", ",", -1, -1);
Symbol.TextualConvention = new Symbol("", "TEXTUAL-CONVENTION", -1, -1);
Symbol.Syntax = new Symbol("", "SYNTAX", -1, -1);
Symbol.Integer = new Symbol("", "INTEGER", -1, -1);
Symbol.Bits = new Symbol("", "BITS", -1, -1);
Symbol.Octet = new Symbol("", "OCTET", -1, -1);
Symbol.String = new Symbol("", "STRING", -1, -1);
Symbol.OpenParentheses = new Symbol("", "(", -1, -1);
Symbol.CloseParentheses = new Symbol("", ")", -1, -1);
Symbol.Exports = new Symbol("", "EXPORTS", -1, -1);
Symbol.DisplayHint = new Symbol("", "DISPLAY-HINT", -1, -1);
Symbol.Status = new Symbol("", "STATUS", -1, -1);
Symbol.Description = new Symbol("", "DESCRIPTION", -1, -1);
Symbol.Reference = new Symbol("", "REFERENCE", -1, -1);
Symbol.DoubleDot = new Symbol("", "..", -1, -1);
Symbol.Integer32 = new Symbol("", "Integer32", -1, -1);
Symbol.IpAddress = new Symbol("", "IpAddress", -1, -1);
Symbol.Counter32 = new Symbol("", "Counter32", -1, -1);
Symbol.TimeTicks = new Symbol("", "TimeTicks", -1, -1);
Symbol.Opaque = new Symbol("", "Opaque", -1, -1);
Symbol.Counter64 = new Symbol("", "Counter64", -1, -1);
Symbol.Unsigned32 = new Symbol("", "Unsigned32", -1, -1);
Symbol.Gauge32 = new Symbol("", "Gauge32", -1, -1);
Symbol.Size = new Symbol("", "SIZE", -1, -1);
Symbol.Units = new Symbol("", "UNITS", -1, -1);
Symbol.MaxAccess = new Symbol("", "MAX-ACCESS", -1, -1);
Symbol.Access = new Symbol("", "ACCESS", -1, -1);
Symbol.Index = new Symbol("", "INDEX", -1, -1);
Symbol.Augments = new Symbol("", "AUGMENTS", -1, -1);
Symbol.DefVal = new Symbol("", "DEFVAL", -1, -1);
Symbol.Of = new Symbol("", "OF", -1, -1);

/**
* Lexer.cs 
*/
function Lexer() { };
Lexer._index = 0;
Lexer._symbols = new Array();
Lexer._symbols.Add = function (_Symbol) {
    Lexer._symbols[Lexer._index] = _Symbol;
    Lexer._index++;
}
Lexer._buffer = new CharBuffer();
Lexer._stringSection = new Boolean();
Lexer._assignSection = new Boolean();
Lexer._assignAhead = new Boolean();
Lexer._dotSection = new Boolean();
Lexer._singleDashFound = new Boolean();
Lexer._commentSection = new Boolean();
    /// <summary>
    /// Parses MIB file to symbol list.
    /// </summary>
    /// <param name="file">File</param>
    /// <param name="stream">File stream</param>
Lexer.prototype.Parse = function (file) {
    Lexer._assignAhead = false;
    Lexer._assignSection = false;
    Lexer._stringSection = false;
    var lines = fs.readFileSync(file).toString().split('\n');
    var line;
    var i = 0;
    while ((line = lines[i]) != null && i <= lines.length) {
        
        this.ParseLine(file, line, i);
        i++;
    }
}
Lexer.prototype.ParseLine = function (file, line, row) {
        line = line + "\n";
        //console.log(line);
        for (var i = 0; i < line.length; i++) {
            var current = line.charAt(i);
            
            var moveNext = this.ParseB(file, current, row, i); //bool
            if (moveNext) {
                break;
            }
        }

        // IMPORTANT: comment does not span lines.
        Lexer._commentSection = false;
        Lexer._singleDashFound = false;
    }
Lexer.prototype.ParseB = function(file, current, row, column){
            switch (current)
            {
                case '\n':
                    if (!Lexer._stringSection)
                    {
                        Lexer._buffer.Fill(Lexer._symbols, file, row, column);
                        Lexer._symbols.Add(this.CreateSpecialSymbol(file, current, row, column));
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
                    if (Lexer._commentSection)
                    {
                        break;
                    }

                    if (!Lexer._stringSection)
                    {
                        Lexer._buffer.Fill(Lexer._symbols, file, row, column);
                        Lexer._symbols.Add(this.CreateSpecialSymbol(file, current, row, column));
                        return false;
                    }

                    break;
                case '"':
                    if (Lexer._commentSection)
                    {
                        break;
                    }

                    Lexer._stringSection = !Lexer._stringSection;
                    break;
                case '-':
                    if (Lexer._stringSection)
                    {
                        break;
                    }

                    if (!Lexer._singleDashFound)
                    {
                        Lexer._singleDashFound = true;
                        break;
                    }

                    Lexer._singleDashFound = false;
                    Lexer._commentSection = !Lexer._commentSection;
                    break;
                case '\r':
                    return false;
                default:
                    if (current == 0x1A)
                    {
                        // IMPORTANT: ignore invisible characters such as SUB.
                        return false;
                    }

                    Lexer._singleDashFound = false;
                    if (Char.IsWhiteSpace(current) && !Lexer._assignSection && !Lexer._stringSection && !Lexer._commentSection)
                    {
                        Lexer._buffer.Fill(Lexer._symbols, file, row, column);
                        return false;
                    }

                    if (Lexer._commentSection)
                    {
                        // TODO: ignore everything here in comment
                        break;
                    }

                    if (Lexer._assignAhead)
                    {
                        Lexer._assignAhead = false;
                        Lexer._buffer.Fill(Lexer._symbols, file, row, column);
                        break;
                    }

                    if (Lexer._dotSection && current != '.')
                    {
                        Lexer._buffer.Fill(Lexer._symbols, file, row, column);
                        Lexer._dotSection = false;
                    }

                    if (current == '.' && !Lexer._stringSection)
                    {
                        if (!Lexer._dotSection)
                        {
                            Lexer._buffer.Fill(Lexer._symbols, file, row, column);
                            Lexer._dotSection = true;
                        }
                    }

                    if (current == ':' && !Lexer._stringSection)
                    {
                        if (!Lexer._assignSection)
                        {
                            Lexer._buffer.Fill(Lexer._symbols, file, row, column);
                        }

                        Lexer._assignSection = true;
                    }

                    if (current == '=' && !Lexer._stringSection)
                    {
                        Lexer._assignSection = false;
                        Lexer._assignAhead = true;
                    }

                    break;
            }

            Lexer._buffer.Append(current);
            return false;
        }
Lexer.prototype.CreateSpecialSymbol = function (file, value, row, column) {
        var str;
        switch (value)
        {
            case '\n':
                str = '\n';
                break;
            case '{':
                str = "{";
                break;
            case '}':
                str = "}";
                break;
            case '(':
                str = "(";
                break;
            case ')':
                str = ")";
                break;
            case '[':
                str = "[";
                break;
            case ']':
                str = "]";
                break;
            case ';':
                str = ";";
                break;
            case ',':
                str = ",";
                break;
            case '|':
                str = "|";
                break;
            default:
                throw new Error("value is not a special character");
        }
        return new Symbol(file, str, row, column);
}
Lexer.prototype.GetNextNonEOLSymbol = function () {
    var result;
    while ((result = this.GetNextSymbol()) == Symbol.EOL) {
    }
    console.log(result);
    return result; //Symbol
}
Lexer.prototype.GetNextSymbol = function () {

    while (Lexer._index < Lexer._symbols.Count) {
        var next = Lexer._symbols[Lexer._index++];
        if (next.IsComment()) {
            continue;
        }
        return next; //Symbol
    }
    return null;
}

/**
* MibDocument.cs
*/
function MibDocument(lexer){
    var temp;
    while ((temp = lexer.GetNextNonEOLSymbol()) != null)
    {
        //temp.ValidateIdentifier();
        _modules.Add(new MibModule(temp.ToString(), lexer));                
    }
}
MibDocument._modules = new Array();
MibDocument._modules._index = 0;
MibDocument._modules.Add = function (mibmodule) {
    _symbols[MibDocument._modules._index] = _Symbol;
    MibDocument._modules._index++;
}

/**
* MibModule.cs
*/
function MibModule(name, lexer){
            if (name == null)
            {
                throw new Error("name");
            }
            
            if (lexer == null)
            {
                throw new Error("lexer");
            }
            
            _name = name.toUpperCase(); // all module name are uppercase.
            var temp = lexer.GetNextNonEOLSymbol();
            temp.Expect(Symbol.Definitions);
            temp = lexer.GetNextNonEOLSymbol();
            temp.Expect(Symbol.Assign);
            temp = lexer.GetNextSymbol();
            temp.Expect(Symbol.Begin);
            temp = lexer.GetNextNonEOLSymbol();
            if (temp == Symbol.Imports)
            {
                //_imports = ParseDependents(lexer);
            }
            else if (temp == Symbol.Exports)
            {
                //_exports = ParseExports(lexer);
            }
            else if (temp == Symbol.End)
            {
                return;
            }

            //ParseEntities(_tokens, temp, _name, lexer);
        }





    var lexer = new Lexer();
    lexer.Parse('RFC_BASE_MINIMUM/RFC1212.MIB');
    console.log(lexer);
    var doc = new MibDocument(lexer);
    //lex.Parse('RFC_BASE_MINIMUM/RFC1155-SMI.MIB');
    /*
    lex.Parse('RFC_BASE_MINIMUM/RFC1215.MIB');
    lex.Parse('RFC_BASE_MINIMUM/RFC1213-MIB-II.MIB');
    lex.Parse('RFC_BASE_MINIMUM/SNMPv2-SMI-V1SMI.MIB');
    lex.Parse('RFC_BASE_MINIMUM/SNMPv2-TC-V1SMI.MIB');
    lex.Parse('RFC_BASE_MINIMUM/SNMPv2-MIB-V1SMI.MIB');
    lex.Parse('RFC_BASE_MINIMUM/IANAifType-MIB-V1SMI.MIB');
    lex.Parse('RFC_BASE_MINIMUM/IF-MIB-V1SMI.MIB');
    lex.Parse('RFC_BASE_MINIMUM/HOST-RESOURCES-MIB-V1SMI.MIB');
    lex.Parse('RFC_BASE_MINIMUM/RFC1514-HOSTS.MIB');
    lex.Parse('RFC_BASE_MINIMUM/LMMIB2.MIB');
    lex.Parse('RFC_BASE_MINIMUM/BRIDGE-MIB.MIB');
    lex.Parse('RFC_BASE_MINIMUM/Printer-MIB.MIB');
    lex.Parse('RFC_BASE_MINIMUM/MSFT.MIB');
    lex.Parse('RFC_BASE_MINIMUM/CISCO-SMI-V1SMI.MIB');
    lex.Parse('RFC_BASE_MINIMUM/CISCO-PRODUCTS-MIB-V1SMI.MIB');
    lex.Parse('RFC_BASE_MINIMUM/CISCO-TC-V1SMI.MIB');
    lex.Parse('RFC_BASE_MINIMUM/CISCO-VTP-MIB-V1SMI.MIB');
    lex.Parse('RFC_BASE_MINIMUM/CISCO-STACK-MIB-V1SMI.MIB');
    lex.Parse('RFC_BASE_MINIMUM/CISCO-CDP-MIB-V1SMI_edit.my');
    */
    console.log(Lexer._symbols);






 








