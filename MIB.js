/// <summary>
///http://tools.ietf.org/html/rfc2578
///http://cpansearch.perl.org/src/FTASSIN/SNMP-MIB-Compiler-0.04/lib/SNMP/MIB/Compiler.pm
///http://www.rfc-editor.org/rfc/pdfrfc/rfc1442.txt.pdf
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

    var next = null;
    while (this._index < this._symbols.length) {
        next = this._symbols[this._index++];
        if (next.IsComment()) {
            //console.log("\t\t" + next.text);
            continue;
        }
        //console.log(next.text);
        if (next.text != "") {
            return next;
        }
    }
    //console.log("---------END----------" + next.text);
    return next;
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

var lexer = new Lexer();
lexer.Parse('RFC_BASE_MINIMUM/RFC1155-SMI.MIB');
//lexer.Parse('RFC_BASE_MINIMUM/RFC1212.MIB');
//lexer.Parse('RFC_BASE_MINIMUM/RFC1215.MIB');
//lexer.Parse('RFC_BASE_MINIMUM/RFC1213-MIB-II.MIB');
/*
lexer.Parse('RFC_BASE_MINIMUM/SNMPv2-SMI-V1SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/SNMPv2-TC-V1SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/SNMPv2-MIB-V1SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/IANAifType-MIB-V1SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/IF-MIB-V1SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/HOST-RESOURCES-MIB-V1SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/RFC1514-HOSTS.MIB');
lexer.Parse('RFC_BASE_MINIMUM/LMMIB2.MIB');
lexer.Parse('RFC_BASE_MINIMUM/BRIDGE-MIB.MIB');
lexer.Parse('RFC_BASE_MINIMUM/Printer-MIB.MIB');
lexer.Parse('RFC_BASE_MINIMUM/MSFT.MIB');
lexer.Parse('RFC_BASE_MINIMUM/CISCO-SMI-V1SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/CISCO-PRODUCTS-MIB-V1SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/CISCO-TC-V1SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/CISCO-VTP-MIB-V1SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/CISCO-STACK-MIB-V1SMI.MIB');
lexer.Parse('RFC_BASE_MINIMUM/CISCO-CDP-MIB-V1SMI_edit.my');
*/
console.log(lexer._symbols.length);
///Variable ::= Value
var Buffer = [];
var Value = [];
var Variable = [];
var CharString = "";
var VariableString = "";
var ValueString = "";
var EOL_Index = 0;
var Assign_Index = 0;
var Buffer_Index = 0;
var VariableBuffer = [];
var ValueBuffer = [];
VariableBuffer.push([Symbol.EOL]); //index adjust /// We key off of Assign but the the Symbol comes after the Variable
var temp = lexer.GetNextSymbol();
while (temp != null) {
    if (temp.text == Symbol.EOL.text) {                     ///   EOL (End of Line)
        EOL_Index = Buffer_Index;
        //Buffer_Index++;
        //Buffer.push(temp);
    }
    else if (temp.text == Symbol.Assign.text || temp.text == Symbol.End.text) {             ///   ::= Assingment or End
        Assign_Index = Buffer_Index;
        Buffer_Index++;
        Buffer.push(temp);

        Variable = [];                                      /// Init Variable buffer
        for (var i = EOL_Index; i <= Assign_Index; i++) {   /// Assemble all Variables from Buffer(EOL<-----Variables----->Assign)
            Variable[i] = Buffer[i];
        }

        Value = [];                                         /// Init Value buffer             
        for (var i = 0; i < EOL_Index; i++) {               /// Assemble all Value from Buffer(0<-----Values----->EOL)
            Value[i] = Buffer[i];
            if (Value[i].text == Symbol.CloseBracket.text) {/// CloseBracket indicates end of Values and start of Variables
                i++;
                for (i; i < EOL_Index; i++) {               /// Assemble all Variables from Buffer(CloseBracket<-----Variables----->EOL)
                    Variable[i] = Buffer[i];
                }
                break;
            }
        }


        Buffer = [];                                       /// Init Buffer
        Buffer_Index = 0;                                  /// Init Buffer_Index
        VariableBuffer.push(Variable);
        ValueBuffer.push(Value);
        VariableString = "";
        ValueString = "";                                   /// Reset CharString
    }
    else if (temp.text == Symbol.Begin.text || temp.text == Symbol.End.text) {
        Buffer_Index++;
        Buffer.push(temp);

    }
    else {
        Buffer_Index++;
        Buffer.push(temp);
    }
    temp = lexer.GetNextSymbol();
};
ValueBuffer.push([Symbol.EOL]); //index adjust

var VarString = "";
var ValString = "";
for (var i = 0; i < VariableBuffer.length; i++) {
    for (var ii = 0; ii < VariableBuffer[i].length; ii++) {
        if (VariableBuffer[i][ii] != null) {
            VarString += VariableBuffer[i][ii].text + " ";
        }
    }
    for (var ii = 0; ii < ValueBuffer[i].length; ii++) {
        if (ValueBuffer[i][ii] != null) {
            ValString += ValueBuffer[i][ii].text + " ";
        }
    }
    //console.log(VarString + "\n\t\t" + ValString);/// Verify reassembly
    ParseObject(VariableBuffer, ValueBuffer);
    /*(
    Typically, there are three kinds of information modules:
    (1) MIB modules, which contain definitions of inter-related
    managed objects, make use of the OBJECT-TYPE and
    NOTIFICATION-TYPE macros;
    (2) compliance statements for MIB modules, which make use of
    the MODULE-COMPLIANCE and OBJECT-GROUP macros [2]; and,
    (3) capability statements for agent implementations which
    make use of the AGENT-CAPABILITIES macros [2].

    3.1. Macro Invocation
    Within an information module, each macro invocation appears
    as:
    <descriptor> <macro> <clauses> ::= <value>
    <VarString> ::= <ValString>
    EX:   
    <descriptor>sysDescr</descriptor>
    <macro>OBJECT-TYPE</macro>
    <clauses>
    SYNTAX DisplayString ( SIZE ( 0 .. 255 ) )
    ACCESS read-only 
    STATUS mandatory 
    DESCRIPTION "A textual description of the network management subsystem"
    </clauses>
    ::=
    <value>{ system 1 }</value>

    3.1.1. Textual Clauses
    Some clauses in a macro invocation may take a textual value
    (e.g., the DESCRIPTION clause).
   
           
    */
    ValString = ""
    VarString = "";
}

function ParseObject(VariableBuffer, ValueBuffer) {
    var temp = "";
    console.log("<MACRO INVOCTION>");
    for (var ii = 0; ii < VariableBuffer[i].length; ii++) {
        if (VariableBuffer[i][ii] != null) {
            temp = VariableBuffer[i][ii].text;
            switch (temp) {
                case Symbol.Object.text:
                    if (VariableBuffer[i][ii + 1].text == "IDENTIFIER") { ii++; }
                    console.log("\t\t<MACRO>", temp + " IDENTIFIER");
                    break;
                case Symbol.Assign.text:
                    break;
                case Symbol.Syntax.text:
                    break;
                case Symbol.End.text:
                    break;
                default:
                    if (Char.IsLetter(temp.charAt(0)) && temp.charAt(0).toUpperCase() == temp.charAt(0)) {
                        /*
                        this is a <macro> or a <clause>.
                        if <macro> does not exist, create it.
                        */
                        console.log("\t\t<MACRO/CLAUSE>", temp);
                    }
                    if (Char.IsLetter(temp.charAt(0)) && temp.charAt(0).toLowerCase() == temp.charAt(0)) {
                        /*
                        this is a <descriptor>.
                        a <macro> and/or <clauses> should follow
                        */
                        console.log("\t<descriptor>", temp);
                    }
                    break;


            }
        }
    }
    temp = "";
    for (var ii = 0; ii < ValueBuffer[i].length; ii++) {
        if (ValueBuffer[i][ii] != null) {
            var symbol = ValueBuffer[i][ii].text;
            switch (symbol) {
                case Symbol.Begin.text:
                    break;
                case Symbol.Exports.text:
                    temp += "{\"" + symbol + "\" : ["; //JSON Array
                    ii++
                    while (ii < ValueBuffer[i].length) {
                        symbol = ValueBuffer[i][ii].text;
                        ii++;
                        if (Char.IsLetter(symbol)) {
                            temp += "\"" + symbol + "\"";
                        }
                        else if (symbol == ";") {
                        //SKIP ;
                        }
                        else {
                            temp += symbol;
                        }

                    }
                    temp += "]}";
                    var obj = eval("(" + temp + ")");
                    console.log(obj.EXPORTS[0]);
                    break
                default:
                    temp += ValueBuffer[i][ii].text + " ";
                    break;
            }
        }
    }
    console.log("\t<value>", temp);
    console.log("</MACRO INVOCTION>");
}


//lexer.Parse('RFC_BASE_MINIMUM/RFC1212.MIB');
//Parse->Parseline->ParseB(fill buffer, create symbols)->

//lexer.Parse('RFC_BASE_MINIMUM/RFC1155-SMI.MIB');
//lexer.Parse('RFC_BASE_MINIMUM/RFC1215.MIB');
//lexer.Parse('RFC_BASE_MINIMUM/RFC1213-MIB-II.MIB');
//console.log(lexer);



/*
var doc = new MibDocument(lexer);
for (var i = 0; i < doc._modules.length; i++) {
console.log("_____________________________________________________________________");
console.log("MibDocument.MibModule[" + i + "]:", doc._modules[i]);
console.log("_____________________________________________________________________");
}
*/


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
