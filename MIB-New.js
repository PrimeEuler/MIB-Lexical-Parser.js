/// <summary>
/// Lexer parses MIB file into Symbol list.
/// Create MibDocument from Lexer.
/// MibDocument creates MibModule (name, imports, exports, tokens) array from Lexer Symbol list.
/// tokens contain: 
/// -OID values and are used to:
/// --SNMP Query network devices for MIB information
/// --SNMP Trap network device MIB notifications
/// </summary>
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
            symbols.push(new Symbol(file, content, row, column));
        }
    }
    return _this;
};
/**
* Symbol.cs 
*/
function Symbol(file, text, row, column) {
    this.file = file;
    this.text = text;
    this.row = row;
    this.column = column;
};
Symbol.prototype.Expect = function (expected) {
    this.Assert(this.text == expected.text, expected + " expected");
}
Symbol.prototype.Assert = function (condition, message) {
    if (condition) {
        return;
    }

}
Symbol.prototype.ToString = function () {
    return this.text;
}
Symbol.prototype.IsComment = function () {
    var symbol = this;
    return symbol != null && (symbol.ToString().indexOf("--") == 0);
}
Symbol.prototype.IsValidIdentifier = function () {
    var symbol = this;
    return Char.IsLetter(this.ToString().charAt(0))
}
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
function Lexer() {
    this._index = 0;
    this._symbols = [];
    this._buffer = new CharBuffer();
    this._stringSection = new Boolean();
    this._assignSection = new Boolean();
    this._assignAhead = new Boolean();
    this._dotSection = new Boolean();
    this._singleDashFound = new Boolean();
    this._commentSection = new Boolean();

    this._stringSection = false;
    this._assignSection = false;
    this._assignAhead = false;
    this._dotSection = false;
    this._singleDashFound = false;
    this._commentSection = false;
};
/// <summary>
/// Parses MIB file to symbol list.
/// </summary>
/// <param name="file">File</param>
/// <param name="stream">File stream</param>
Lexer.prototype.Parse = function (file) {
    this._assignAhead = false;
    this._assignSection = false;
    this._stringSection = false;
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
    for (var i = 0; i < line.length; i++) {
        var current = line.charAt(i);
        var moveNext = this.ParseB(file, current, row, i); //bool
        if (moveNext) {
            break;
        }
    }
    // IMPORTANT: comment does not span lines.
    this._commentSection = false;
    this._singleDashFound = false;
}
Lexer.prototype.ParseB = function (file, current, row, column) {
    switch (current) {
        case '\n':
            if (!this._stringSection) {
                this._buffer.Fill(this._symbols, file, row, column);
                this._symbols.push(this.CreateSpecialSymbol(file, current, row, column));
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
            if (this._commentSection) {
                break;
            }
            if (!this._stringSection) {
                this._buffer.Fill(this._symbols, file, row, column);
                this._symbols.push(this.CreateSpecialSymbol(file, current, row, column));
                return false;
            }
            break;
        case '"':
            if (this._commentSection) {
                break;
            }
            this._stringSection = !this._stringSection;
            break;
        case '-':
            if (this._stringSection) {
                break;
            }

            if (!this._singleDashFound) {
                this._singleDashFound = true;
                break;
            }
            this._singleDashFound = false;
            this._commentSection = !this._commentSection;
            break;
        case '\r':
            return false;
        default:
            if (current == 0x1A) {
                // IMPORTANT: ignore invisible characters such as SUB.
                return false;
            }
            this._singleDashFound = false;
            if (Char.IsWhiteSpace(current) && !this._assignSection && !this._stringSection && !this._commentSection) {
                this._buffer.Fill(this._symbols, file, row, column);
                return false;
            }
            if (this._commentSection) {
                // TODO: ignore everything here in comment
                break;
            }
            if (this._assignAhead) {
                this._assignAhead = false;
                this._buffer.Fill(this._symbols, file, row, column);
                break;
            }
            if (this._dotSection && current != '.') {
                this._buffer.Fill(this._symbols, file, row, column);
                this._dotSection = false;
            }
            if (current == '.' && !this._stringSection) {
                if (!this._dotSection) {
                    this._buffer.Fill(this._symbols, file, row, column);
                    this._dotSection = true;
                }
            }
            if (current == ':' && !this._stringSection) {
                if (!this._assignSection) {
                    this._buffer.Fill(this._symbols, file, row, column);
                }
                this._assignSection = true;
            }
            if (current == '=' && !this._stringSection) {
                this._assignSection = false;
                this._assignAhead = true;
            }
            break;
    }
    //console.log(current);
    this._buffer.Append(current);
    return false;
}
Lexer.prototype.CreateSpecialSymbol = function (file, value, row, column) {
    var str;
    switch (value) {
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
    var result = Symbol.EOL;

    while ((result.text == Symbol.EOL.text)) {
        result = this.GetNextSymbol();

        if (result == null) {
            break;
        }
    }
    return result; //Symbol
}
Lexer.prototype.GetNextSymbol = function () {
    while (this._index < this._symbols.length) {
        var next = this._symbols[this._index++];

        if (next.IsComment()) {
            continue;
        }
        if (next.text != "") {
            return next;
        }
    }
    return null;
}
Lexer.prototype.CheckNextNonEOLSymbol = function () {
    var length = 0;
    while (this._symbols[this._index + length].text == Symbol.EOL.text) {
        length++;
    }
    return this._symbols[this._index + length];
}
Lexer.prototype.CheckNextSymbol = function () {
    return this._symbols[this._index];
}
Lexer.prototype.ParseOidValue = function (OidValueAssignment) {
    var parent = "";
    var value = 0;
    var previous = null;
    var temp = this.GetNextNonEOLSymbol();
    temp.Expect(Symbol.OpenBracket);
    var longParent = "";
    temp = this.GetNextNonEOLSymbol();
    longParent += temp.text;
    while ((temp = this.GetNextNonEOLSymbol()) != null) {

        if (temp.text == Symbol.OpenParentheses.text) {
            longParent += temp.text;
            temp = this.GetNextNonEOLSymbol();
            var succeed = new Boolean();
            value = temp.ToString();
            succeed = Char.IsDigit(value);
            OidValueAssignment._value = value; ///////////////////
            temp.Assert(succeed, "not a decimal");
            longParent += temp.text;
            temp = this.GetNextNonEOLSymbol();
            temp.Expect(Symbol.CloseParentheses);
            longParent += temp.text;
            continue;
        }

        if (temp.text == Symbol.CloseBracket.text) {
            parent = longParent;
            OidValueAssignment._parent = parent; ///////////////////
            OidValueAssignment._value = value; ////////////////////
            return;
        }

        var succeeded = new Boolean();
        value = temp.ToString();
        succeeded = Char.IsDigit(value);
        //console.log("^^^^^^^^^^^^^",parent, value);
        OidValueAssignment._parent = parent;
        OidValueAssignment._value = value;
        if (succeeded) {
            // numerical way
            while ((temp = this.GetNextNonEOLSymbol()).text != Symbol.CloseBracket.text) {
                longParent += "." + temp.ToString();
                value = temp.ToString();
                succeeded = Char.IsDigit(value);
                temp.Assert(succeeded, "not a decimal");
            }
            temp.Expect(Symbol.CloseBracket);
            parent = longParent;
            OidValueAssignment._parent = parent; ///////////////
            OidValueAssignment._value = value; ////////////////
            return;
        }

        longParent += ".";
        longParent += temp.ToString();
        temp = this.GetNextNonEOLSymbol();
        temp.Expect(Symbol.OpenParentheses);
        longParent += temp.ToString();
        temp = this.GetNextNonEOLSymbol();
        succeeded = Char.IsDigit(temp.ToString());
        temp.Assert(succeeded, "not a decimal");
        longParent += temp.ToString();
        temp = this.GetNextNonEOLSymbol();
        temp.Expect(Symbol.CloseParentheses);
        longParent += temp.ToString();
        previous = temp;

    }

    //throw MibException.Create("end of file reached", previous);
}

/**
* MibDocument.cs
*/
function MibDocument(lexer) {
    var temp;
    this._modules = [];
    while ((temp = lexer.GetNextNonEOLSymbol()) != null) {
        if (temp.IsValidIdentifier()) {
            this._modules.push(new MibModule(temp.ToString(), lexer));
        }
    }
}

/**
* MibModule.cs
*/
function MibModule(name, lexer) {
    this._name = "";
    this._imports = "";
    this._exports = "";
    this._tokens = [];

    this._name = name.toUpperCase(); // all module names are uppercase.
    var temp = lexer.GetNextNonEOLSymbol();
    console.log(temp.text + " |||| " + Symbol.Definitions.text);
    temp.Expect(Symbol.Definitions);

    temp = lexer.GetNextNonEOLSymbol();
    console.log(temp.text + " |||| " + Symbol.Assign.text);
    temp.Expect(Symbol.Assign);

    temp = lexer.GetNextSymbol();
    console.log(temp.text + " |||| " + Symbol.Begin.text);
    temp.Expect(Symbol.Begin);


    temp = lexer.GetNextNonEOLSymbol();
    console.log(temp.text + " |||| ");
    if (temp.text == Symbol.Imports.text) {
        this._imports = this.ParseDependents(lexer);
    }
    else if (temp.text == Symbol.Exports.text) {
        this._exports = this.ParseExports(lexer);
    }
    else if (temp.text == Symbol.End.text) {
        return;
    }
    this.ParseEntities(this._tokens, temp, this._name, lexer);
}
MibModule.prototype.ParseEntities = function (tokens, last, module, lexer) {
    var temp = last;
    var buffer = [];
    do {
        if (temp.text == Symbol.Imports.text || temp.text == Symbol.Exports.text || temp.text == Symbol.EOL.text) {
            continue;
        }
        buffer.push(temp);
        if (temp.text != Symbol.Assign.text) {
            continue;
        }
        this.ParseEntity(tokens, module, buffer, lexer);
        buffer = [];
    }
    while (((temp = lexer.GetNextSymbol()).text != Symbol.End.text));
}
MibModule.prototype.ParseEntity = function (tokens, module, buffer, lexer) {
    //buffer[0].Assert(buffer.Count > 1, "unexpected symbol");
    //if (buffer[0].IsValidIdentifier()) 
    //console.log("--------------" + buffer);
    {
        console.log("---------------------------------------------\n", buffer );
        if (buffer.length == 2) {
            tokens.push(this.ParseOthers(module, buffer, lexer));
            console.log("\nParseEntity.tokens.push(ParseOthers)\n", tokens[tokens.length - 1]);
        }
        else if (buffer[1].text == Symbol.Object.text) {
            tokens.push(this.ParseObjectIdentifier(module, buffer, lexer));
            console.log("\nParseEntity.tokens.push(ParseObjectIdentifier)\n", tokens[tokens.length - 1]);
        }
        else if (buffer[1].text == Symbol.ModuleIdentity.text) {
            tokens.push(new ModuleIdentity(module, buffer, lexer));
        }
        else if (buffer[1].text == Symbol.ObjectType.text) {
            tokens.push(new ObjectType(module, buffer, lexer));
        }
        else if (buffer[1].text == Symbol.ObjectGroup.text) {
            tokens.push(new ObjectGroup(module, buffer, lexer));
        }
        else if (buffer[1].text == Symbol.NotificationGroup.text) {
            tokens.push(new NotificationGroup(module, buffer, lexer));
        }
        else if (buffer[1].text == Symbol.ModuleCompliance.text) {
            tokens.push(new ModuleCompliance(module, buffer, lexer));
        }
        else if (buffer[1].text == Symbol.NotificationType.text) {
            tokens.push(new NotificationType(module, buffer, lexer));
        }
        else if (buffer[1].text == Symbol.ObjectIdentity.text) {
            tokens.push(new ObjectIdentity(module, buffer, lexer));
        }
        else if (buffer[1].text == Symbol.Macro.text) {
            tokens.push(new Macro(module, buffer, lexer));
            console.log("\nParseEntity.tokens.push(Macro)\n", tokens[tokens.length - 1]);
        }
        else if (buffer[1].text == Symbol.TrapType.text) {
            tokens.push(new TrapType(module, buffer, lexer));
        }
        else if (buffer[1].text == Symbol.AgentCapabilities.text) {
            tokens.push(new AgentCapabilities(module, buffer, lexer));
        }
    }
}
MibModule.prototype.ParseObjectIdentifier = function (module, header, lexer) {
    header[0].Assert(header.length == 4, "invalid OID value assignment");
    header[2].Expect(Symbol.Identifier);
    return new OidValueAssignment(module, header[0].ToString(), lexer);
}
MibModule.prototype.ParseDependents = function (lexer) {
    return new Imports(lexer);
}
MibModule.prototype.ParseOthers = function (module, header, lexer) {
    var current = lexer.GetNextNonEOLSymbol();

    if (current.text == Symbol.Sequence.text) {
        return new Sequence(module, header[0].ToString(), lexer);
    }

    if (current.text == Symbol.Choice.text) {
        return new Choice(module, header[0].ToString(), lexer);
    }

    if (current.text == Symbol.Integer.text) {
        return new IntegerType(module, header[0].ToString(), lexer);
    }

    if (current.text == Symbol.TextualConvention.text) {
        return new TextualConvention(module, header[0].ToString(), lexer);
    }

    return new TypeAssignment(module, header[0].ToString(), current, lexer);
}
MibModule.prototype.ParseExports = function (lexer) {
    return new Exports(lexer);
}
/**
* Imports.cs
*/
function Imports(lexer) {
    this._dependents = [];
    var temp;
    while ((temp = lexer.GetNextSymbol()).text != Symbol.Semicolon.text) {
        if (temp.text == Symbol.EOL.text) {
            continue;
        }

        this._dependents.push(new ImportsFrom(temp, lexer).Module());
    }
}
/**
* ImportsFrom.cs
*/
function ImportsFrom(last, lexer) {
    this._module = "";
    this._types = [];
    var previous = last;
    var temp;
    while ((temp = lexer.GetNextSymbol()).text != Symbol.From.text) {
        if (temp.text == Symbol.EOL.text) {
            continue;
        }

        if (temp.text == Symbol.Comma.text) {
            //previous.ValidateIdentifier();
            //console.log(previous.ToString());
            this._types.push(previous.ToString());
        }

        previous = temp;
    }

    this._module = lexer.GetNextSymbol().ToString().toUpperCase(); // module names are uppercase
}
ImportsFrom.prototype.Module = function () {
    return this._module;
}
/**
* Exports.cs
*/
function Exports(lexer) {
    this._types = [];
    var previous = null;
    var temp;
    while ((temp = lexer.GetNextSymbol()).text != Symbol.Semicolon.text) {
        if (temp.text == Symbol.EOL.text) {
            continue;
        }

        if (temp.text == Symbol.Comma.text && previous.text != null) {
            //previous.ValidateIdentifier();
            this._types.push(previous.ToString());
        }

        previous = temp;
    }
}

/**
* OidValueAssignment.cs
*/
function OidValueAssignment(module, name, lexer) {
    this._module = module;
    this._name = name;
    this._parent = "";
    this._value = "";
    lexer.ParseOidValue(this);
}


/**
* ObjectType.cs
* TODO-ParseSyntax,ParseUnits,ParseAccess,ParseStatus,ParseDescription,ParseReference,ParseIndices,ParseAugments,ParseDefVal
*/
function ObjectType(module, header, lexer) {
    this._module = module;
    this._parent="";
    this._value=0;
    this._name = header[0].ToString();
    this._syntax="";
    this._units = "";
    this._access = "";
    this._status = "";
    this._description = "";
    this._reference = "";
    this._indices = "";
    this._augment = "";
    this._defVal = "";
    this.ParseProperties(header);//group of symbols = header
    lexer.ParseOidValue(this);
    

}
ObjectType.prototype.ParseProperties = function (header) {

            this._syntax = "ParseSyntax"; //ParseSyntax(_module, _name, enumerator, ref temp);
            this._units = "ParseUnits"; //ParseUnits(enumerator, ref temp);
            this._access = "ParseAccess"; //ParseAccess(enumerator, ref temp);
            this._status = "ParseStatus"; //ParseStatus(enumerator, ref temp);
            this._description = "ParseDescription"; //ParseDescription(enumerator, ref temp);
            this._reference = "ParseReference"; //ParseReference(enumerator, ref temp);
            this._indices = "ParseIndices"; //ParseIndices(enumerator, ref temp);
            this._augment = "ParseAugments"; //ParseAugments(enumerator, ref temp);
            this._defVal = "ParseDefVal"; //ParseDefVal(enumerator, ref temp);

}
/**
* ModuleIdentity.cs
*/
function ModuleIdentity(module, header, lexer) {
    this._module = module;
    this._name = header[0].ToString();
    this._parent = "";
    this._value = "";
    lexer.ParseOidValue(this);
}
/**
* ObjectGroup.cs
*/
function ObjectGroup(module, header, lexer) {
    this._module = module;
    this._name = header[0].ToString();
    this._parent = "";
    this._value = "";
    lexer.ParseOidValue(this);
}
/**
* ObjectIdentity.cs
*/
function ObjectIdentity(module, header, lexer) {
    this._module = module;
    this._name = header[0].ToString();
    this._parent = "";
    this._value = "";
    lexer.ParseOidValue(this);
    if (this._parent == "0") {
        this._parent = "ccitt";
    }
}
/**
* NotificationGroup.cs
*/
function NotificationGroup(module, header, lexer) {
    this._module = module;
    this._name = header[0].ToString();
    this._parent = "";
    this._value = "";
    lexer.ParseOidValue(this);
}
/**
* ModuleCompliance.cs
*/
function ModuleCompliance(module, header, lexer) {
    this._module = module;
    this._name = header[0].ToString();
    this._parent = "";
    this._value = "";
    lexer.ParseOidValue(this);
}
/**
* NotificationType.cs
*/
function NotificationType(module, header, lexer) {
    this._module = module;
    this._name = header[0].ToString();
    this._parent = "";
    this._value = "";
    lexer.ParseOidValue(this);
}
/**
* TrapType.cs
*/
function TrapType(module, header, lexer) {
    this._module = module;
    this._name = header[0].ToString();
    var temp;
    while ((temp.text = lexer.GetNextSymbol().text) == Symbol.EOL.text) {
    }

    this._value = temp.ToString();
    var succeeded = new Boolean() = Char.IsDigit(value);
    temp.Assert(succeeded, "not a decimal");
}
/**
* AgentCapabilities.cs
*/
function AgentCapabilities(module, header, lexer) {
    this._module = module;
    this._name = header[0].ToString();
    this._parent = "";
    this._value = "";
    lexer.ParseOidValue(this);
}
/**
* Macro.cs
*/
function Macro(module, header, lexer) {
    this._name = header[0].ToString();
    var temp;
    while ((temp = lexer.GetNextSymbol()).text != Symbol.Begin.text) {
    }

    while ((temp = lexer.GetNextSymbol()).text != Symbol.End.text) {
    }
}
/**
* Choice.cs
*/
function Choice(module, name, lexer) {
    this._name = name;

    var temp;
    while ((temp = lexer.GetNextSymbol()).text != Symbol.OpenBracket.text) {
    }

    while ((temp = lexer.GetNextSymbol()).text != Symbol.CloseBracket.text) {
    }
}
/**
* IntegerType.cs
*/
function IntegerType(module, name, lexer) {
    this._name = name;
    this._isEnumeration = new Boolean() = false;
    this._map = [];
    this._ranges = [];
    var temp = lexer.CheckNextNonEOLSymbol();
    if (temp.text == Symbol.OpenBracket.text) {
        lexer.GetNextNonEOLSymbol();
        this._isEnumeration = true;
        this._map = DecodeEnumerations(lexer); ////////////////
    }
    else if (temp.text == Symbol.OpenParentheses.text) {
        lexer.GetNextNonEOLSymbol();
        this._isEnumeration = false;
        this._ranges = DecodeRanges(lexer); ///////////////
    }
}
/**
* Sequence.cs
*/
function Sequence(module, name, lexer) {
    this._name = name;
    // parse between ( )
    var temp = lexer.GetNextNonEOLSymbol();
    var bracketSection = 0;
    temp.Expect(Symbol.OpenBracket);
    bracketSection++;
    while (bracketSection > 0) {
        temp = lexer.GetNextNonEOLSymbol();
        if (temp.text == Symbol.OpenBracket.text) {
            bracketSection++;
        }
        else if (temp.text == Symbol.CloseBracket.text) {
            bracketSection--;
        }
    }
}
/**
* TextualConvention.cs
* TODO-DisplayHint,StatusHelper.CreateStatus,BitsType,IntegerType,OctetStringType,IpAddressType,Counter64Type,UnsignedType,ObjectIdentifierType,CustomType
*/
function TextualConvention(module, name, lexer){
            this._name = name;
            this._displayHint;
            this._status;
            this._description;
            this._reference;
            this._syntax;

            var temp = lexer.GetNextNonEOLSymbol();

            if (temp.text == Symbol.DisplayHint.text){
                // TODO: this needs decoding to a useful format.
                this._displayHint = new DisplayHint(lexer.GetNextNonEOLSymbol().ToString().replace(/"/gi,""));//Trim(new[] { '"' }));
                temp = lexer.GetNextNonEOLSymbol();
            }

            temp.Expect(Symbol.Status);
            try{
                this._status = StatusHelper.CreateStatus(lexer.GetNextNonEOLSymbol().ToString());
                temp = lexer.GetNextNonEOLSymbol();
            }
            catch (ArgumentException){
                temp.Throw("Invalid status");
            }

            temp.Expect(Symbol.Description);
            this._description = lexer.GetNextNonEOLSymbol().ToString().replace(/"/gi,"");//Trim(new[] { '"' });
            temp = lexer.GetNextNonEOLSymbol();

            if (temp.text == Symbol.Reference.text){
                this._reference = lexer.GetNextNonEOLSymbol().ToString();
                temp = lexer.GetNextNonEOLSymbol();
            }

            temp.Expect(Symbol.Syntax);

            /* 
             * RFC2579 definition:
             *       Syntax ::=   -- Must be one of the following:
             *                    -- a base type (or its refinement), or
             *                    -- a BITS pseudo-type
             *               type
             *             | "BITS" "{" NamedBits "}"
             *
             * From section 3.5:
             *      The data structure must be one of the alternatives defined
             *      in the ObjectSyntax CHOICE or the BITS construct.  Note
             *      that this means that the SYNTAX clause of a Textual
             *      Convention can not refer to a previously defined Textual
             *      Convention.
             *      
             *      The SYNTAX clause of a TEXTUAL CONVENTION macro may be
             *      sub-typed in the same way as the SYNTAX clause of an
             *      OBJECT-TYPE macro.
             * 
             * Therefore the possible values are (grouped by underlying type):
             *      INTEGER, Integer32
             *      OCTET STRING, Opaque
             *      OBJECT IDENTIFIER
             *      IpAddress
             *      Counter64
             *      Unsigned32, Counter32, Gauge32, TimeTicks
             *      BITS
             * With appropriate sub-typing.
             */

            temp = lexer.GetNextNonEOLSymbol();
            if (temp.text == Symbol.Bits.text){
                this._syntax = new BitsType(module, string.Empty, lexer);
            }
            else if (temp.text == Symbol.Integer.text || temp.text == Symbol.Integer32.text){
                this._syntax = new IntegerType(module, string.Empty, lexer);
            }
            else if (temp.text == Symbol.Octet.text){
                temp = lexer.GetNextSymbol();
                temp.Expect(Symbol.String);
                this._syntax = new OctetStringType(module, string.Empty, lexer);
            }
            else if (temp.text == Symbol.Opaque.text){
                this._syntax = new OctetStringType(module, string.Empty, lexer);
            }
            else if (temp.text == Symbol.IpAddress.text){
                this._syntax = new IpAddressType(module, string.Empty, lexer);
            }
            else if (temp.text == Symbol.Counter64.text){
                this._syntax = new Counter64Type(module, string.Empty, lexer);
            }
            else if (temp.text == Symbol.Unsigned32.text || temp.text == Symbol.Counter32.text || temp.text == Symbol.Gauge32.text || temp.text == Symbol.TimeTicks.text){
                this._syntax = new UnsignedType(module, string.Empty, lexer);
            }
            else if (temp.text == Symbol.Object.text){
                temp = lexer.GetNextSymbol();
                temp.Expect(Symbol.Identifier);
                this._syntax = new ObjectIdentifierType(module, string.Empty, lexer);
            }
            else
            {
                //temp.Throw("illegal syntax for textual convention");
                this._syntax = new CustomType(module, string.Empty, lexer);
            }
        }

/**
* TypeAssignment.cs
*/
 function TypeAssignment(module, name, last, lexer) {
     this._module = module;
     this._name = name;
     var temp = lexer.CheckNextSymbol();
     this.Value = last.ToString();
     var previous = last; 
     var veryPrevious = previous;
     console.log(temp.text);
     while ((temp = lexer.CheckNextSymbol()) != null) {
     //while (temp != null) {
         if (veryPrevious.text == Symbol.EOL.text && previous.text == Symbol.EOL.text && temp.IsValidIdentifier()) {
             
             return;
         }
         veryPrevious = previous;
         temp = lexer.GetNextSymbol();
         previous = temp;
         
     }
     //console.log("TypeAssignment:end of file reached");
     //previous.Throw("end of file reached");
 }






var lexer = new Lexer();
lexer.Parse('RFC_BASE_MINIMUM/RFC1155-SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/RFC1212.MIB');
//Parse->Parseline->ParseB(fill buffer, create symbols)->

//lexer.Parse('RFC_BASE_MINIMUM/RFC1155-SMI.MIB');
//lexer.Parse('RFC_BASE_MINIMUM/RFC1215.MIB');
//lexer.Parse('RFC_BASE_MINIMUM/RFC1213-MIB-II.MIB');
//console.log(lexer);
var doc = new MibDocument(lexer);
for (var i = 0; i < doc._modules.length; i++) {
    console.log("_____________________________________________________________________");
    console.log("MibDocument.MibModule[" + i + "]:", doc._modules[i]);
    console.log("_____________________________________________________________________");
}
/// <summary>
/// Lexer parses MIB file into Symbol list.
/// Create MibDocument from Lexer.
/// MibDocument creates MibModule (name, imports, exports, tokens) array from Lexer Symbol list.
/// tokens contain OID values and are used to:
/// -SNMP Query network devices for MIB information
/// -SNMP Trap network device MIB notifications
/// </summary>



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
//console.log(doc._modules);







