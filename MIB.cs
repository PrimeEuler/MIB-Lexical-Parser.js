namespace SCAN
{

    using System;
    using System.Collections;
    //using System.Collections.Generic;
    using System.Text;
    using System.Net.Sockets;
    using System.Net;
    using System.Threading;
    using System.Data;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.XPath;
    using System.Runtime.InteropServices;
    using System.Data.SqlClient;

    public class MIBScanner : System.Web.UI.Page
    {
        public class Network
        {
            #region public voids
            /// <summary>
            /// Removes empty strings in a string array.
            /// </summary>
            public static void RemoveEmpty(ref String[] Temp)
            {
                int x = 0;
                for (int ti = 0; ti < Temp.Length; ti++)
                {
                    if (Temp[ti].Trim().Length > 0)
                    {
                        x++;
                    }
                }
                String[] TMP = new string[x];
                x = 0;
                for (int ti = 0; ti < Temp.Length; ti++)
                {
                    if (Temp[ti].Trim().Length > 0)
                    {
                        TMP[x] = Temp[ti].Trim();
                        x++;
                    }
                }
                Temp = TMP;
            }
            /// <summary>
            /// Returns a hexidecimal string from an array of bytes.
            /// </summary>
            public static String toHex(byte[] b)
            {
                if (b == null)
                {
                    return null;
                }
                String[] hits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
                String sb = "";

                for (int i = 0; i < b.Length; i++)
                {
                    int j = ((int)b[i]) & 0xFF;
                    sb += (hits[(j / 16)]);
                    sb += (hits[(j % 16)]);
                }
                return sb;
            }
            /// <summary>
            /// Returns a UInt32 from an array of network order bytes.
            /// </summary>
            public static UInt32 GetU32(Byte[] B)
            {
                if (B.Length == 5)
                {
                    Array.Reverse(B);
                    return (BitConverter.ToUInt32(B, 0));
                }
                else if (B.Length < 5)
                {
                    byte[] u32 = new Byte[4];
                    //Array.Reverse(B);

                    int i = B.Length;

                    while (i < 4)
                    {
                        u32[i] = 0;
                        i++;
                    }
                    Array.Copy(B, 0, u32, 4 - B.Length, B.Length);
                    Array.Reverse(u32);
                    return (BitConverter.ToUInt32(u32, 0));
                }
                else if (B.Length > 5)
                {
                    byte[] u32 = new Byte[4];
                    Array.Copy(B, u32, 4);
                    Array.Reverse(u32);
                    return (BitConverter.ToUInt32(u32, 0));

                }
                else
                {
                    return (0);
                }

                //int16_);
            }
            #endregion


            /// <summary>
            /// Base class for Managment Information Base (MIB) RFC 2578.
            /// </summary>
            public class MIB
            {
                #region Private Members
                /// <summary>
                /// Directory where the "RFC_BASE_MINIMUM" folder with the base MIB files exists.
                /// </summary>
                public static String dir = "";
                /// <summary>
                /// COntents of the enterprise-numbers.txt file
                /// </summary>
                private static String Vendors = "";
                /// <summary>
                /// Reads Contents of the enterprise-numbers.txt file to the private Vendor String
                /// </summary>
                private static void Import_Enterprise(String FileName)
                {
                    StreamReader SR = File.OpenText(dir + FileName);
                    Vendors = SR.ReadToEnd();
                }
                /// <summary>
                /// Finds the Vendors Name based on the number provided.
                /// </summary>
                private String Get_Vendor(int I)
                {
                    String line = null;
                    StringReader SR = new StringReader(Vendors);
                    line = SR.ReadLine();
                    int x = 0;
                    bool t = false;
                    while (line != null)
                    {
                        try
                        {
                            x = int.Parse(line);
                            t = true;
                        }
                        catch
                        {
                            t = false;
                        }

                        if (t && line.IndexOf(" ") == -1 && x == I)
                        {
                            MIB A = this;
                            MIB B = new MIB();
                            MIB OB = new MIB();
                            OB = new MIB();
                            OB.ParentName = "enterprises";
                            OB.ParentIndex = x;
                            OB.Name = SR.ReadLine().Trim();
                            A.AddChild(ref OB, ref B);
                            return SR.ReadLine().Trim();
                        }
                        line = SR.ReadLine();
                    }
                    return line;
                }
                /// <summary>
                /// Parses and Imports MIBs from .MIB files.
                /// </summary>
                private class MIBParser
                {
                    private static String Vendors = "";
                    private static String DIR = "";
                    #region TOKENS
                    private static String[] TOKEN = {
                    "--",                   //  0
                    "{",                    //  1
                    "}",                    //  2
                    "::=",                  //  3
                    ";",                    //  4  
                    "OBJECT IDENTIFIER",    //  5
                    "-TYPE",                  //  6
                    "-IDENTITY",              //  7
                    "-GROUP",                 //  8
                    "-COMPLIANCE",            //  9
                    "XXXXXXXXXXX",            //  10
                    "XXXXXXXXXXX",            //  11
                    "XXXXXXXXXXX",            //  12
                    "BEGIN",                //13
                    "IMPORTS",              //14
                    "DEFINITIONS",          //15
                    "SYNTAX",               //16
                    "ACCESS",               //17
                    "MAX-ACCESS",           //18
                    "MIN-ACCESS",           //19       
                    "STATUS",               //20
                    "DESCRIPTION",          //21
                    "REFERENCE",            //22
                      "INDEX",              //23       
                    "SEQUENCE",             //  9
                    "FROM",             
                    "END",              
                    "INTEGER",          
                    "OCTET STRING",     
                    "TEXTUAL-CONVENTION",
                    "OBJECT-GROUP",
                    "DEFVAL",
                    "LAST-UPDATED",
                    "ORGANIZATION",
                    "CONTACT-INFO",
                    "REVISION",
                    "ENTERPRISE"
                };
                    #endregion TOKENS
                    struct MIB_OBJECT
                    {
                        public String ObjectName;
                        public String ParentName;
                        public int ParentIndex;
                        public String[] GrandParentsName;
                        public int[] GrandParentsIndex;
                        public String DEFINITION;
                        public String BODY;
                        public String TYPE;
                        public String SYNTAX;
                        public String OPTION;
                        public String ACCESS;
                        public String INDEX;
                        public String DESCRIPTION;
                        public String IMPORTS;

                    }
                    private static void Import_MIB(String FileName)
                    {
                        Console.WriteLine(DIR + FileName);
                        StreamReader SR = File.OpenText(DIR + FileName);
                        String line = null;
                        String oldline = null;
                        String IMPORTS = "";
                        String EXPORTS = "";
                        String BEGINS = "app";
                        MIB OB = new MIB();
                        line = SR.ReadLine();
                        while (line != null)
                        {
                            if (line != null)
                            {
                                line = line.Trim();
                                REM(ref SR, ref line);
                                BEGIN(ref SR, ref line, ref BEGINS);
                                IMPORT(ref SR, ref line, ref IMPORTS);
                                EXPORT(ref SR, ref line, ref EXPORTS);
                                BUILD_OBJECT_A(ref SR, ref line, ref OB, ref oldline);
                                BUILD_OBJECT_B(ref SR, ref line, ref OB, ref BEGINS, ref IMPORTS);
                            }
                            oldline = line;
                            line = SR.ReadLine();
                        }

                    }
                    public static void Import_MIB(String FileName, ref MIB A)
                    {
                        Console.WriteLine(DIR + FileName);
                        StreamReader SR = File.OpenText(DIR + FileName);
                        String line = null;
                        String oldline = null;
                        String IMPORTS = "";
                        String EXPORTS = "";
                        String BEGINS = "app";
                        MIB OB = new MIB();
                        line = SR.ReadLine();
                        while (line != null)
                        {
                            if (line != null)
                            {
                                line = line.Trim();
                                REM(ref SR, ref line);
                                BEGIN(ref SR, ref line, ref BEGINS);
                                IMPORT(ref SR, ref line, ref IMPORTS);
                                EXPORT(ref SR, ref line, ref EXPORTS);
                                BUILD_OBJECT_A(ref SR, ref line, ref OB, ref oldline);
                                BUILD_OBJECT_B(ref SR, ref line, ref OB, ref BEGINS, ref IMPORTS, ref A);
                            }
                            oldline = line;
                            line = SR.ReadLine();
                        }

                    }
                    public static void Import_Enterprise(String FileName)
                    {
                        StreamReader SR = File.OpenText(DIR + FileName);
                        Vendors = SR.ReadToEnd();
                    }
                    public static String Get_Vendor(int I)
                    {
                        String line = null;
                        StringReader SR = new StringReader(Vendors);
                        line = SR.ReadLine();
                        int x = 0;
                        bool t = false;
                        while (line != null)
                        {
                            try
                            {
                                x = int.Parse(line);
                                t = true;
                            }
                            catch
                            {
                                t = false;
                            }

                            if (t && line.IndexOf(" ") == -1 && x == I)
                            {
                                MIB OB = new MIB();
                                OB = new MIB();
                                OB.ParentName = "enterprises";
                                OB.ParentIndex = x;
                                OB.Name = SR.ReadLine().Trim();
                                BUILD_MIB(OB);
                                return SR.ReadLine().Trim();
                            }
                            line = SR.ReadLine();
                        }
                        return line;
                    }
                    private static void REM(ref StreamReader SR, ref String line)// REMOVE LINES CONTAINING "--"
                    {
                        if (line.IndexOf("--") > -1 && line.IndexOf("---") == -1)
                        {
                            line = line.Substring(0, line.IndexOf("--"));
                        }
                        line = line.Replace("\t", " ");
                    }
                    private static void BEGIN(ref StreamReader SR, ref String line, ref String BEGINS)// FIND BEGINS
                    {
                        if (line.IndexOf(TOKEN[13]) > -1)
                        {
                            BEGINS = line.Substring(0, line.IndexOf(TOKEN[15])).Trim();
                            line = "";
                        }
                    }
                    private static void IMPORT(ref StreamReader SR, ref String line, ref String IMPORTS)// BUILD IMPORTS
                    {
                        if (line.IndexOf(TOKEN[14]) > -1)
                        {
                            while (line.IndexOf(TOKEN[4]) == -1)
                            {
                                line = SR.ReadLine().Trim();
                                REM(ref SR, ref line);
                                if (line.IndexOf("FROM ") > -1)
                                {
                                    IMPORTS += line.Substring(line.IndexOf("FROM ") + 5).Replace(";", "") + ",";
                                }
                            }
                            FIND_IMPORTS(IMPORTS);
                            line = "";
                        }

                    }
                    private static void EXPORT(ref StreamReader SR, ref String line, ref String EXPORTS)// BUILD IMPORTS
                    {
                        if (line.IndexOf("EXPORTS") > -1)
                        {
                            while (line.IndexOf(TOKEN[4]) == -1)
                            {
                                line = SR.ReadLine().Trim();
                                REM(ref SR, ref line);
                                EXPORTS += line;
                            }
                            // Console.WriteLine(EXPORTS.Replace(";",""));
                            line = "";
                        }

                    }
                    private static void FIND_IMPORTS(String IMPORTS)
                    {
                        String[] IM = IMPORTS.Split(",".ToCharArray());
                        foreach (String S in IM)
                        {
                            if (S.Trim().Length > 0)
                            {
                                //if (app.GetElementsByTagName(S)[0] == null)
                                {
                                    // Console.WriteLine("Cant find:\t" + S);
                                };
                            }
                        }
                    }
                    private static void BUILD_OBJECT_A(ref StreamReader SR, ref String line, ref MIB OB, ref String oldline)// BUILD OBJECT
                    {
                        int X = 0;
                        for (int T = 5; T <= 12; T++)
                        {
                            X = line.IndexOf(TOKEN[T]);

                            if (X > -1)
                            {
                                OB = new MIB();
                                String ObjectType = "";
                                if (line == "OBJECT-TYPE")
                                {
                                    OB.Name = oldline;
                                    ObjectType = line;
                                    //Console.WriteLine(line);
                                }
                                else
                                {

                                    OB.Name = line.Substring(0, line.IndexOf(" ")).Trim();
                                    ObjectType = line.Remove(0, line.IndexOf(" ")).Trim();
                                    if (ObjectType == null)
                                    {
                                        //Console.WriteLine(OB.ObjectName);
                                    }
                                }


                                if (X < line.IndexOf(TOKEN[3]))
                                { ObjectType = ObjectType.Substring(0, ObjectType.IndexOf(TOKEN[3])).Trim(); }
                                else if (line.IndexOf(TOKEN[3]) > -1)
                                { OB.SYNTAX = ObjectType.Remove(0, ObjectType.IndexOf(TOKEN[3]) + TOKEN[3].Length).Trim(); ObjectType = null; }

                                OB.TYPE = ObjectType;
                                OB.BODY = line.Remove(0, X + TOKEN[T].Length);
                                while (line.IndexOf(TOKEN[3]) == -1)
                                {
                                    line = SR.ReadLine().Trim();
                                    REM(ref SR, ref line);
                                    BUILD_SYNTAX(ref SR, ref line, ref OB);
                                    BUILD_ACCESS(ref SR, ref line, ref OB);
                                    BUILD_INDEX(ref SR, ref line, ref OB);
                                    BUILD_DESCRIPTION(ref SR, ref line, ref OB);
                                    OB.BODY += "\t" + line + "\r\n";
                                }
                                break;
                            }
                        }
                    }
                    private static void BUILD_SYNTAX(ref StreamReader SR, ref String line, ref MIB OB)
                    {
                        if (line.Trim() == "SYNTAX")
                        {
                            line = "SYNTAX " + SR.ReadLine().Trim();
                        }
                        int SYNTAX = line.IndexOf("SYNTAX");
                        if (SYNTAX > -1)
                        {

                            String List = "";
                            String ListName = "";
                            if (line.IndexOf("{") > -1)
                            {
                                BUILD_LIST(ref SR, ref line, ref ListName, ref List);
                            }
                            if (List.Length > 0)
                            {
                                OB.SYNTAX += ListName.Remove(0, SYNTAX + "SYNTAX".Length).Trim();
                                OB.OPTION += List.Trim().Replace("{", "").Replace("}", "");
                            }
                            else
                            {
                                OB.SYNTAX += line.Remove(0, SYNTAX + TOKEN[16].Length).Trim();
                            }
                        }
                    }
                    private static void BUILD_ACCESS(ref StreamReader SR, ref String line, ref MIB OB)
                    {
                        int ACCESS = line.IndexOf(TOKEN[17]);
                        if (ACCESS > -1)
                        {
                            String List = "";
                            String ListName = "";
                            if (line.IndexOf("{") > -1)
                            {
                                BUILD_LIST(ref SR, ref line, ref ListName, ref List);
                            }
                            OB.ACCESS = "";
                            if (List.Length > 0)
                            {
                                OB.ACCESS += ListName.Remove(0, ACCESS + TOKEN[17].Length).Trim();
                                OB.ACCESS += List.Trim();
                            }
                            else
                            {
                                OB.ACCESS += line.Remove(0, ACCESS + TOKEN[17].Length).Trim();
                            }
                        }
                    }
                    private static void BUILD_INDEX(ref StreamReader SR, ref String line, ref MIB OB)
                    {
                        int INDEX = line.IndexOf(TOKEN[23]);
                        if (INDEX > -1)
                        {
                            String List = "";
                            String ListName = "";
                            if (line.IndexOf("{") > -1)
                            {
                                BUILD_LIST(ref SR, ref line, ref ListName, ref List);
                            }
                            if (List.Length > 0)
                            {
                                OB.INDEX += ListName.Remove(0, INDEX + TOKEN[23].Length).Trim();
                                OB.INDEX += List.Trim();

                            }
                            else
                            {
                                OB.INDEX += line.Remove(0, INDEX + TOKEN[23].Length).Trim();
                            }
                        }
                    }
                    private static void BUILD_DESCRIPTION(ref StreamReader SR, ref String line, ref MIB OB)
                    {
                        int DESCRIPTION = line.IndexOf(TOKEN[21]);
                        if (DESCRIPTION > -1)
                        {

                            int X = line.IndexOf("\"");                     //FIND ""'""
                            while (X < 0)
                            {
                                line = SR.ReadLine().Trim();
                                REM(ref SR, ref line);
                                X = line.IndexOf("\"");
                                line = line.Remove(0, X + 1);

                            }
                            int XX = line.IndexOf("\"");
                            while (XX < 0)                                  //FIND "}"
                            {
                                OB.DESCRIPTION += line + "\r\n";
                                line = SR.ReadLine().Trim();
                                REM(ref SR, ref line);
                                XX = line.IndexOf("\"");
                            }
                            OB.DESCRIPTION += line.Substring(0, XX);

                        }
                    }
                    private static void BUILD_OBJECT_B(ref StreamReader SR, ref String line, ref MIB OB, ref String BEGINS, ref String IMPORTS)// BUILD OBJECT
                    {
                        String[] Name_Type;
                        String Name = "";
                        String Type = "";
                        String Part3 = "";
                        String Part4 = "";
                        int X = 0;
                        X = line.IndexOf(TOKEN[3]);
                        if (X > -1)
                        {
                            Name_Type = line.Substring(0, X).Trim().Split(" ".ToCharArray());//".......::="
                            if (Name_Type.Length > 0)
                            {
                                for (int tt = 0; tt < Name_Type.Length; tt++)
                                {
                                    if (Name_Type[tt].Trim().Length > 0)
                                    {
                                        Name = Name_Type[tt].Trim();
                                        break;
                                    }
                                }

                            }
                            Type = line.Remove(0, (X + TOKEN[3].Length)).Trim();    //"::=......."

                            if (OB.Name == null)
                            {
                                String tmp = null;
                                BUILD_OBJECT_A(ref SR, ref Name, ref OB, ref tmp);
                                if (OB.Name == null)
                                {
                                    OB = new MIB();
                                    OB.Name = Name;
                                }
                            }
                            if (OB.Name != null)
                            {
                                while (Type.Length == 0)
                                {
                                    line = SR.ReadLine().Trim();
                                    REM(ref SR, ref line);
                                    Type += line;
                                }
                                if (Type.IndexOf("{") > -1)
                                {
                                    String ListName = "";
                                    String List = "";
                                    BUILD_LIST(ref SR, ref Type, ref ListName, ref List);
                                    if (ListName.Length > 0) { Part3 = ListName; }
                                    if (List.Length > 0) { Part4 = List; }

                                }
                                else
                                {
                                    Part3 = Type;
                                }
                                if (OB.TYPE == null)//RFC EXPORT
                                {
                                    OB.TYPE = BEGINS;
                                    OB.SYNTAX = Part3;
                                    OB.OPTION = Part4.Replace("{", "").Replace("}", "");
                                }
                                else if (Part4.Length > 0)//HAS PARENT
                                {
                                    Part4 = Part4.Replace("{", "").Replace("}", "");
                                    String[] Temp = Part4.Split(" ".ToCharArray());
                                    RemoveEmpty(ref Temp);
                                    if (Part4.IndexOf("(") > -1)
                                    {
                                        OB.GrandParentsName = new String[Temp.Length - 1];
                                        OB.GrandParentsIndex = new int[Temp.Length];
                                        for (int ti = 0; ti < Temp.Length; ti++)
                                        {
                                            if (Temp[ti].IndexOf("(") > -1)
                                            {
                                                OB.GrandParentsName[ti] = Temp[ti].Substring(0, Temp[ti].IndexOf("("));
                                                String TT = Temp[ti].Replace(OB.GrandParentsName[ti] + "(", "");
                                                TT = TT.Replace(")", "");
                                                OB.GrandParentsIndex[ti] = int.Parse(TT);
                                            }
                                            else if (ti < Temp.Length - 1)
                                            {
                                                OB.GrandParentsName[ti] = Temp[ti].Trim();
                                            }
                                        }
                                        OB.ParentName = OB.GrandParentsName[Temp.Length - 2];
                                    }
                                    else
                                    {
                                        for (int tt = 0; tt < Temp.Length; tt++)
                                        {
                                            if (Temp[tt].Trim().Length > 0)
                                            {
                                                OB.ParentName = Temp[tt];
                                                break;
                                            }
                                        }
                                    }
                                    for (int idx = 0; idx < Temp.Length; idx++)
                                    {
                                        try
                                        {
                                            OB.ParentIndex = int.Parse(Temp[idx]);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                OB.IMPORTS = IMPORTS;
                                OB.DEFINITION = BEGINS;
                                BUILD_MIB(ref OB);
                                OB = new MIB();
                            }
                        }
                    }
                    private static void BUILD_OBJECT_B(ref StreamReader SR, ref String line, ref MIB OB, ref String BEGINS, ref String IMPORTS, ref MIB A)// BUILD OBJECT
                    {
                        String[] Name_Type;
                        String Name = "";
                        String Type = "";
                        String Part3 = "";
                        String Part4 = "";
                        int X = 0;
                        X = line.IndexOf(TOKEN[3]);
                        if (X > -1)
                        {
                            Name_Type = line.Substring(0, X).Trim().Split(" ".ToCharArray());//".......::="
                            if (Name_Type.Length > 0)
                            {
                                for (int tt = 0; tt < Name_Type.Length; tt++)
                                {
                                    if (Name_Type[tt].Trim().Length > 0)
                                    {
                                        Name = Name_Type[tt].Trim();
                                        break;
                                    }
                                }

                            }
                            Type = line.Remove(0, (X + TOKEN[3].Length)).Trim();    //"::=......."

                            if (OB.Name == null)
                            {
                                String tmp = null;
                                BUILD_OBJECT_A(ref SR, ref Name, ref OB, ref tmp);
                                if (OB.Name == null)
                                {
                                    OB = new MIB();
                                    OB.Name = Name;
                                }
                            }
                            if (OB.Name != null)
                            {
                                while (Type.Length == 0)
                                {
                                    line = SR.ReadLine().Trim();
                                    REM(ref SR, ref line);
                                    Type += line;
                                }
                                if (Type.IndexOf("{") > -1)
                                {
                                    String ListName = "";
                                    String List = "";
                                    BUILD_LIST(ref SR, ref Type, ref ListName, ref List);
                                    if (ListName.Length > 0) { Part3 = ListName; }
                                    if (List.Length > 0) { Part4 = List; }

                                }
                                else
                                {
                                    Part3 = Type;
                                }
                                if (OB.TYPE == null)//RFC EXPORT
                                {
                                    OB.TYPE = BEGINS;
                                    OB.SYNTAX = Part3;
                                    OB.OPTION = Part4.Replace("{", "").Replace("}", "");
                                }
                                else if (Part4.Length > 0)//HAS PARENT
                                {
                                    Part4 = Part4.Replace("{", "").Replace("}", "");
                                    String[] Temp = Part4.Split(" ".ToCharArray());
                                    RemoveEmpty(ref Temp);
                                    if (Part4.IndexOf("(") > -1)
                                    {
                                        OB.GrandParentsName = new String[Temp.Length - 1];
                                        OB.GrandParentsIndex = new int[Temp.Length];
                                        for (int ti = 0; ti < Temp.Length; ti++)
                                        {
                                            if (Temp[ti].IndexOf("(") > -1)
                                            {
                                                OB.GrandParentsName[ti] = Temp[ti].Substring(0, Temp[ti].IndexOf("("));
                                                String TT = Temp[ti].Replace(OB.GrandParentsName[ti] + "(", "");
                                                TT = TT.Replace(")", "");
                                                OB.GrandParentsIndex[ti] = int.Parse(TT);
                                            }
                                            else if (ti < Temp.Length - 1)
                                            {
                                                OB.GrandParentsName[ti] = Temp[ti].Trim();
                                            }
                                        }
                                        OB.ParentName = OB.GrandParentsName[Temp.Length - 2];
                                    }
                                    else
                                    {
                                        for (int tt = 0; tt < Temp.Length; tt++)
                                        {
                                            if (Temp[tt].Trim().Length > 0)
                                            {
                                                OB.ParentName = Temp[tt];
                                                break;
                                            }
                                        }
                                    }
                                    for (int idx = 0; idx < Temp.Length; idx++)
                                    {
                                        try
                                        {
                                            OB.ParentIndex = int.Parse(Temp[idx]);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                OB.IMPORTS = IMPORTS;
                                OB.DEFINITION = BEGINS;
                                BUILD_MIB(ref OB, ref A);
                                OB = new MIB();
                            }
                        }
                    }
                    private static void BUILD_LIST(ref StreamReader SR, ref String line, ref String ListName, ref String List)// BUILD OBJECT
                    {
                        int X = 0;
                        X = line.IndexOf(TOKEN[1]);                     //FIND "{"
                        int XX = line.IndexOf(TOKEN[2]);                //FIND "}"
                        List = line.Remove(0, X).Trim();       //"{"
                        ListName = line.Substring(0, X).Trim();               //"....{"
                        while (XX < 0)                                  //FIND "}"
                        {
                            line = SR.ReadLine().Trim();
                            REM(ref SR, ref line);
                            if (line.IndexOf(TOKEN[0]) > -1) { line = line.Substring(0, line.IndexOf(TOKEN[0])) + "\r\n"; }
                            if (line.Length > 0) { List += line + "\r\n"; }                            //"{.......}"
                            XX = List.IndexOf(TOKEN[2]);
                        }
                    }
                    public static MIB MIBS = new MIB();
                    private static MIB B = new MIB();
                    private static void BUILD_MIB(MIB OB)
                    {
                        if (OB.ParentName != null)
                        {
                            MIBS.ParentIndex = 1;
                            MIBS.Name = "iso";

                            // if (B.Name == OB.ParentName)//if OB is a child of B add to B
                            {
                                //   B.AddChild(ref OB, ref B);//Ref parent for faster node selection
                            }
                            //  else
                            {
                                MIBS.AddChild(ref OB, ref B);//Add OB and Ref parent node to B
                            }
                        }

                    }
                    private static void BUILD_MIB(ref MIB OB)
                    {
                        //if (OB.ParentName != null)
                        {
                            MIBS.ParentIndex = 1;
                            MIBS.Name = "iso";

                            //if (B.Name == OB.ParentName)//if OB is a child of B add to B
                            {
                                //B.AddChild(ref OB, ref B);//Ref parent for faster node selection
                            }
                            // else
                            {
                                MIBS.AddChild(ref OB, ref B);//Add OB and Ref parent node to B
                            }
                            //BUILD_XML(ref OB);
                        }

                    }
                    private static void BUILD_MIB(ref MIB OB, ref MIB A)
                    {
                        A.AddChild(ref OB, ref B);
                    }
                    static MIBParser()
                    {
                    }
                }
                private String GetRFC(String SYNTAX)
                {
                    MIB A = null;
                    A = this;
                    String S = "";
                    for (int X = 0; X < A.RFCS.Length; X++)
                    {
                        for (int Z = 0; Z < A.RFCS[X].RFCS.Length; Z++)
                        {
                            S = A.RFCS[X].RFCS[Z].Name;
                            if (S == SYNTAX)
                            {
                                if (A.RFCS[X].RFCS[Z].OPTION != null)
                                {
                                    return A.RFCS[X].RFCS[Z].OPTION;
                                }
                            }
                        }
                    }
                    return null;
                }
                #endregion


                /// <summary>
                /// Load MIBS from the RFC_BASE_MINIMUM" folder.
                /// </summary>
                public void LoadFromBaseMIBS()
                {


                    MIB A = this;
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//RFC1212.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//RFC1155-SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//RFC1215.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//RFC1213-MIB-II.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//SNMPv2-SMI-V1SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//SNMPv2-TC-V1SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//SNMPv2-MIB-V1SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//IANAifType-MIB-V1SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//IF-MIB-V1SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//HOST-RESOURCES-MIB-V1SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//RFC1514-HOSTS.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//LMMIB2.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//BRIDGE-MIB.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//Printer-MIB.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//MSFT.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//CISCO-SMI-V1SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//CISCO-PRODUCTS-MIB-V1SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//CISCO-TC-V1SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//CISCO-VTP-MIB-V1SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//CISCO-STACK-MIB-V1SMI.MIB", ref A);
                    MIBParser.Import_MIB(dir + "RFC_BASE_MINIMUM//CISCO-CDP-MIB-V1SMI_edit.my", ref A);
                }
                /// <summary>
                /// Imports MIBs from the specified file.
                /// </summary>
                public void Import_MIB(String FileName)
                {
                    MIB A = this;
                    MIBParser.Import_MIB(FileName, ref A);
                }

                /// <summary>
                /// Parent MIB of current MIB.
                /// </summary>
                public MIB Parent;
                /// <summary>
                /// Parent MIB's Name.
                /// </summary>
                public String ParentName;
                /// <summary>
                /// The index on the Parent MIB that current MIB resides.
                /// </summary>
                public int ParentIndex;
                /// <summary>
                /// GrandParentsName array.
                /// </summary>
                public String[] GrandParentsName;
                /// <summary>
                /// GrandParentsIndex array.
                /// </summary>
                public int[] GrandParentsIndex;
                /// <summary>
                /// Current MIBs Name.
                /// </summary>
                public String Name;
                /// <summary>
                /// Child MIBs.
                /// </summary>
                public MIB[] OIDS = new MIB[1];
                /// <summary>
                /// Child RFCs. RFCs store objects are only related to the Base MIB 
                /// </summary>
                public MIB[] RFCS = new MIB[0];
                /// <summary>
                /// DEFINITION file of current MIB 
                /// </summary>
                public String DEFINITION;
                /// <summary>
                /// BODY of current MIB 
                /// </summary>
                public String BODY;
                /// <summary>
                /// TYPE of current MIB 
                /// </summary>
                public String TYPE;
                /// <summary>
                /// SYNTAX of current MIB 
                /// </summary>
                public String SYNTAX;
                /// <summary>
                /// OPTION information for SYNTAX of current MIB 
                /// </summary>
                public String OPTION;
                /// <summary>
                /// ACCESS level of current MIB 
                /// </summary>
                public String ACCESS;
                /// <summary>
                /// INDEX of current MIB 
                /// </summary>
                public String INDEX;
                /// <summary>
                /// DESCRIPTION of current MIB 
                /// </summary>
                public String DESCRIPTION;
                /// <summary>
                /// IMPORTS  section of current RFC 
                /// </summary>
                public String IMPORTS;
                /// <summary>
                /// A flag to set so that this MIB is used in the SCAN;
                /// </summary>
                public bool SCAN = false;

                /// <summary>
                /// Inits the MIB class and loads the enterprise-numbers.txt file for Vendor look up
                /// <para>String DIR</para>Directory where the "RFC_BASE_MINIMUM" folder with the base MIB files exists.
                /// </summary>
                public MIB(String DIR)
                {
                    dir = DIR;
                    Import_Enterprise("RFC_BASE_MINIMUM//enterprise-numbers.txt");
                    try
                    {
                        this.LoadFromXml(dir + "mib.xml");
                    }
                    catch
                    {
                    }
                    if (this.OIDS.Length == 1)
                    {
                        this.LoadFromBaseMIBS();
                        this.SaveToXml(dir + "mib.xml");
                    }
                    this.SaveToXmlTreeView(dir + "treeview.xml");

                }
                public MIB()
                {
                }
                public void Save()
                {
                    this.SaveToXml(dir + "mib.xml");
                    this.SaveToXmlTreeView(dir + "treeview.xml");
                }
                public void Reset()
                {
                    this.OIDS = new MIB[1];
                    this.RFCS = new MIB[0];
                    this.LoadFromBaseMIBS();
                    this.SaveToXml(dir + "mib.xml");
                    this.SaveToXmlTreeView(dir + "treeview.xml");
                }

                /// <summary>
                /// Add a Child MIB to the current MIB;
                /// </summary>
                private void AddChild(ref MIB newChild, ref MIB refNode)
                {
                    #region RFC
                    if (newChild.ParentName == null || newChild.ParentName == newChild.DEFINITION || newChild.ParentName == "rfc")
                    {

                        if (newChild.ParentName == null)
                        {
                            newChild.ParentName = newChild.DEFINITION;
                        }
                        bool RFC_Found = false;
                        bool RFC_Child_Found = false;
                        for (int RFC_ID = 0; RFC_ID < this.RFCS.Length; RFC_ID++)
                        {
                            if (this.RFCS[RFC_ID] != null && this.RFCS[RFC_ID].Name == newChild.ParentName)//find parent rfc
                            {
                                RFC_Found = true;

                                for (int cID = 0; cID < this.RFCS[RFC_ID].RFCS.Length; cID++)
                                {
                                    if (this.RFCS[RFC_ID].RFCS[cID].Name == newChild.Name)
                                    {
                                        RFC_Child_Found = true;
                                        break;
                                    }
                                }

                                if (!RFC_Child_Found)
                                {
                                    int l = this.RFCS[RFC_ID].RFCS.Length;
                                    MIB[] tmp = new MIB[l + 1];
                                    newChild.Parent = this.RFCS[RFC_ID];
                                    newChild.ParentIndex = l + 1;
                                    this.RFCS[RFC_ID].RFCS.CopyTo(tmp, 0);
                                    this.RFCS[RFC_ID].RFCS = tmp;
                                    this.RFCS[RFC_ID].RFCS[l] = newChild;//add the child
                                }
                                break;
                            }
                        }

                        if (!RFC_Found)// cant find parent create it
                        {
                            int L = this.RFCS.Length;
                            MIB[] tmp = new MIB[L + 1];
                            this.RFCS.CopyTo(tmp, 0);
                            this.RFCS = tmp;

                            if (newChild.ParentName == "rfc")
                            {
                                this.RFCS[L] = newChild;
                                this.RFCS[L].Parent = this;
                            }
                            else
                            {
                                this.RFCS[L] = new MIB();
                                this.RFCS[L].Name = newChild.ParentName;
                                this.RFCS[L].ParentName = "rfc";
                                this.RFCS[L].ParentIndex = L;
                                this.RFCS[L].Parent = this;

                                int LZ = this.RFCS[L].RFCS.Length;
                                newChild.Parent = this.RFCS[L];
                                newChild.ParentIndex = LZ + 1;
                                MIB[] tmpy = new MIB[LZ + 1];
                                this.RFCS[L].RFCS.CopyTo(tmpy, 0);
                                this.RFCS[L].RFCS = tmpy;
                                this.RFCS[L].RFCS[LZ] = newChild;
                            }
                        }


                        return;
                    }
                    #endregion
                    MIB A = null;
                    MIB B = null;
                    A = this;
                    if (refNode.Name == newChild.ParentName)
                    {
                        A = refNode;
                    }
                    for (int G = 0; G <= A.OIDS.Length; G++)
                    {

                        while (A.OIDS.Length == G && A != null)//while End of oids go up one level
                        {
                            G = A.ParentIndex + 1;
                            A = A.Parent;
                            if (A == null) { break; }
                        }
                        if (A == null) //if Parent is null Add Grandgarents
                        {

                            A = this;
                            Console.WriteLine("Cant find " + newChild.ParentName + " for\t" + newChild.Name);
                            int GP = 0;
                            if (newChild.GrandParentsName != null)
                            {
                                for (; GP < newChild.GrandParentsName.Length - 1; GP++)
                                {

                                    B = new MIB();
                                    B.ParentName = newChild.GrandParentsName[GP];
                                    B.Name = newChild.GrandParentsName[GP + 1];
                                    B.ParentIndex = newChild.GrandParentsIndex[GP + 1];
                                    A.AddChild(ref B, ref A);
                                }
                                B = new MIB();
                                B.ParentName = newChild.GrandParentsName[GP];
                                B.ParentIndex = newChild.GrandParentsIndex[GP + 1];
                                B.Name = newChild.ParentName;
                                A.AddChild(ref B, ref A);
                                A.AddChild(ref refNode, ref A);
                            }
                            else
                            {
                                //Console.WriteLine("NO grandparents " + newChild.ParentName);
                            }
                            //A = null;
                            break;
                        }
                        while (A.OIDS[G] != null && A.Name != newChild.ParentName)//while current oid != null search for Newchilds Parent
                        {
                            A = A.OIDS[G];
                            if (A.Name == newChild.ParentName) { break; }//break when found!!
                            G = 0;
                        }
                        if (A.Name == newChild.ParentName)//Found Parent
                        {
                            //Console.WriteLine(newChild.ParentName + " found! \t Add : " + newChild.Name);
                            break;
                        }
                    }
                    if (A != null)//A is parent
                    {
                        if (A.OIDS.Length <= newChild.ParentIndex)//if newChild.ParentIndex > than current lenght add lenght
                        {
                            MIB[] tmp = new MIB[newChild.ParentIndex + 1];
                            A.OIDS.CopyTo(tmp, 0);
                            A.OIDS = tmp;
                        }
                        if (A.OIDS[newChild.ParentIndex] == null)//if the ParentIndex oid is null add new Child
                        {
                            newChild.Parent = A;
                            A.OIDS[newChild.ParentIndex] = newChild;
                            refNode = A;
                        }
                        else if (A.OIDS[newChild.ParentIndex].Name == newChild.Name)//if names match update the option
                        {
                            newChild.Parent = A;
                            refNode = A;
                            if (newChild.SYNTAX != null)
                            {
                                String OPT = GetRFC(newChild.SYNTAX);
                                if (OPT != null)
                                {
                                    A.OIDS[newChild.ParentIndex].OPTION = OPT;
                                }
                            }
                        }
                        else if (A.OIDS[newChild.ParentIndex].Name != newChild.Name)
                        {
                            newChild.Parent = A;
                            A.OIDS[newChild.ParentIndex] = newChild;

                        }
                    }


                }

                /// <summary>
                /// Returns a OID string from a givin input;
                /// </summary>
                public String OIDtoMIB(int[] oid)
                {
                    MIB A = null;
                    MIB B = null;
                    A = this;
                    int i = 0;
                    String mibs = "iso.";
                    for (int S = 1; S < oid.Length; S++)
                    {
                        try
                        {
                            B = A.OIDS[oid[S]];
                            mibs += B.Name + ".";
                            //Console.WriteLine(B.Mib);
                            A = B;
                        }
                        catch
                        {
                            mibs += oid[S] + ".";
                        }
                    }
                    return mibs;
                }
                public String OIDtoMIB(String oidS, ref MIB B)
                {
                    String[] oid = oidS.Split(".".ToCharArray());
                    MIB A = null;
                    //ARR B = null;
                    A = this;
                    int i = 0;
                    String mib = null;
                    for (int S = 1; S < oid.Length; S++)
                    {
                        i = int.Parse(oid[S]);
                        if (i >= A.OIDS.Length)
                        {
                            break;
                        }
                        else
                        {
                            B = A.OIDS[i];
                            if (B == null) { break; }
                            if (B.OIDS.Length == 1)
                            {
                                mib = B.Name;
                                break;
                            }
                            A = B;
                        }
                    }
                    return mib;
                }
                public String OIDtoMIB(String oidS)
                {
                    String[] oid = oidS.Split(".".ToCharArray());
                    MIB A = null;
                    MIB B = null;
                    A = this;
                    int i = 0;
                    String mib = null;
                    for (int S = 1; S < oid.Length; S++)
                    {
                        i = int.Parse(oid[S]);
                        if (i >= A.OIDS.Length)
                        {
                            break;
                        }
                        else
                        {
                            B = A.OIDS[i];
                            if (B == null) { break; }
                            if (B.OIDS.Length == 1 || S == oid.Length - 1)
                            {
                                mib = B.Name;
                                break;
                            }
                            A = B;
                        }
                    }
                    return mib;
                }

                /// <summary>
                /// Returns true if a MIB is found from the given input;
                /// </summary>
                public bool OID_FOUND(String _OID, out String OID, out String _oid, out String _mib, ref object Value)
                {
                    String[] oid = _OID.Split(".".ToCharArray());
                    MIB A = null;
                    MIB B = null;
                    A = this;
                    int i = 0;
                    OID = "1.";
                    _mib = "";
                    _oid = null;
                    bool found = false;
                    //String mibs = "iso.";
                    for (int S = 1; S < oid.Length; S++)
                    {
                        OID += oid[S];
                        i = int.Parse(oid[S]);
                        if (i >= A.OIDS.Length)
                        {
                            break;
                        }
                        else
                        {
                            B = A.OIDS[i];
                            if (B == null) { break; }
                            if (B.OIDS.Length == 1 && B.SCAN == true)
                            {
                                _mib = B.Name;
                                Value = ProcessObject(B, (byte[])Value);
                                found = true;
                                break;
                            }
                            A = B;
                        }
                        OID += ".";
                    }
                    if (OID != _OID)
                    {
                        _oid = _OID.Replace(OID + ".", "");
                    }

                    return (found);
                }
                public bool OID_FOUND(String _OID, out String OID, out String _oid, ref MIB B)
                {
                    String[] oid = _OID.Split(".".ToCharArray());
                    MIB A = null;
                    A = this;
                    int i = 0;
                    OID = "1.";
                    _oid = null;
                    bool found = false;
                    for (int S = 1; S < oid.Length; S++)
                    {
                        OID += oid[S];
                        i = int.Parse(oid[S]);
                        if (i >= A.OIDS.Length)
                        {
                            break;
                        }
                        else
                        {
                            B = A.OIDS[i];
                            if (B == null) { break; }
                            if (B.OIDS.Length == 1)
                            {
                                found = true;
                                break;
                            }
                            A = B;
                        }
                        OID += ".";
                    }
                    if (OID != _OID)
                    {
                        _oid = _OID.Replace(OID + ".", "");
                    }
                    return (found);
                }

                /// <summary>
                /// Returns an array of OIDS taht match the MIB Name given. Optionaly set the SCAN FLAG;
                /// </summary>
                public String[] GetOIDS(String mib, bool scan)
                {
                    MIB A = null;
                    MIB B = null;
                    A = this;
                    int i = 0;
                    String[] oids = new string[0];
                    String[] mib_ = new string[1];
                    String oid = "";
                    String mibs = "";
                    int idx = 0;
                    for (int X = 0; X <= A.OIDS.Length; X++)
                    {

                        while (A.OIDS.Length == X)
                        {
                            X = A.ParentIndex + 1;
                            A = A.Parent;
                            if (A == null) { break; }
                        }

                        if (A == null) { break; }
                        if (A.OIDS[X] != null)
                        {
                            if (A.OIDS[X].Name == mib)
                            //if(A.OIDS[X].OIDS.Length ==1)
                            {
                                B = A.OIDS[X];
                                oid = B.ParentIndex.ToString();
                                mibs = B.Name;
                                B.SCAN = scan;
                                while (B.Name != "iso") //to the top
                                {
                                    B = B.Parent;
                                    oid = B.ParentIndex.ToString() + "." + oid;
                                    mibs = B.Name + "." + mibs;
                                }
                                if (idx >= oids.Length)
                                {
                                    String[] tmp = new string[idx + 1];
                                    oids.CopyTo(tmp, 0);
                                    oids = tmp;
                                }
                                oids[idx] = oid;
                                idx++;
                                oid = "";
                                //break;
                            }
                            A = A.OIDS[X];
                            X = 0;
                        }
                    }
                    return oids;
                }
                public String[] GetOIDS(String mib)
                {
                    MIB A = null;
                    MIB B = null;
                    A = this;
                    int i = 0;
                    String[] oids = new string[0];
                    String[] mib_ = new string[1];
                    String oid = "";
                    String mibs = "";
                    int idx = 0;
                    for (int X = 0; X <= A.OIDS.Length; X++)
                    {

                        while (A.OIDS.Length == X)
                        {
                            X = A.ParentIndex + 1;
                            A = A.Parent;
                            if (A == null) { break; }
                        }

                        if (A == null) { break; }
                        if (A.OIDS[X] != null)
                        {
                            if (A.OIDS[X].Name == mib)
                            //if(A.OIDS[X].OIDS.Length ==1)
                            {
                                B = A.OIDS[X];
                                oid = B.ParentIndex.ToString();
                                mibs = B.Name;
                                while (B.Name != "iso") //to the top
                                {
                                    B = B.Parent;
                                    oid = B.ParentIndex.ToString() + "." + oid;
                                    mibs = B.Name + "." + mibs;
                                }
                                if (idx >= oids.Length)
                                {
                                    String[] tmp = new string[idx + 1];
                                    oids.CopyTo(tmp, 0);
                                    oids = tmp;
                                }
                                oids[idx] = oid;
                                idx++;
                                oid = "";
                                //break;
                            }
                            A = A.OIDS[X];
                            X = 0;
                        }
                    }
                    return oids;
                }
                public String MIBtoOID(String mib, String mibParent)
                {
                    MIB A = null;
                    MIB B = null;
                    A = this;
                    String oid = null;
                    for (int X = 0; X <= A.OIDS.Length; X++)
                    {
                        while (A.OIDS.Length == X)
                        {
                            X = A.ParentIndex + 1;
                            A = A.Parent;
                            if (A == null) { break; }
                        }
                        if (A == null) { break; }
                        if (A.OIDS[X] != null)
                        {
                            if (A.Name == mibParent && A.OIDS[X].Name == mib)
                            {
                                B = A.OIDS[X];
                                oid = B.ParentIndex.ToString();
                                while (B.Name != "iso") //to the top
                                {
                                    B = B.Parent;
                                    oid = B.ParentIndex.ToString() + "." + oid;
                                }
                                return oid;
                            }
                            A = A.OIDS[X];
                            X = 0;
                        }
                    }
                    return oid;
                }

                /// <summary>
                /// Processes the RAW SNMP Object into a value basedd on the current MIB base;
                /// </summary>
                public Object ProcessObject(MIB B, byte[] Value)
                {
                    try
                    {
                        if (B.SYNTAX == null || Value == null) { return null; }
                        else
                            if (B.SYNTAX.ToLower().IndexOf("string") > -1)
                            {
                                byte[] asciiBytes = new byte[Value.Length];
                                for (int ab = 0; ab < asciiBytes.Length; ab++)
                                {
                                    if (Value[ab] > 31)
                                    {
                                        asciiBytes[ab] = Value[ab];
                                    }
                                    else
                                    {
                                        asciiBytes[ab] = 32;
                                    }
                                }

                                return ASCIIEncoding.ASCII.GetString(asciiBytes);
                               // return ASCIIEncoding.Unicode.GetString(Value);
                            }
                            else
                                if (B.SYNTAX.IndexOf("IpAddress") > -1 || B.SYNTAX.IndexOf("NetworkAddress") > -1)
                                {
                                    long l = BitConverter.ToUInt32(Value, 0);
                                    return (new IPAddress(l)).ToString();
                                }
                                else
                                    if (B.SYNTAX.IndexOf("TimeTicks") > -1)
                                    {
                                        return (new TimeSpan((((long)GetU32(Value)) * 100000)).ToString());
                                    }
                                    else
                                        if (B.SYNTAX.IndexOf("PhysAddress") > -1)
                                        {
                                            return (toHex(Value)).ToString();
                                        }
                                        else//OBJECT-IDENTITY
                                            if (B.SYNTAX.IndexOf("AutonomousType") > -1 || B.SYNTAX.IndexOf("OBJECT IDENTIFIER") > -1 || B.SYNTAX.IndexOf("OBJECT-IDENTITY") > -1)
                                            {
                                                String oi = "1.3.6.1.4.1.";
                                                String Vendor = "";
                                                String OB = SNMP.OID(Value);
                                                String[] V = OB.Split(".".ToCharArray());
                                                int e = OB.IndexOf(oi);
                                                if (e > -1)
                                                {
                                                    oi += V[6];
                                                    Vendor = this.OIDtoMIB(oi);
                                                    if (Vendor == null)
                                                    {
                                                        Vendor = Get_Vendor(int.Parse(V[6]));
                                                    }
                                                }
                                                OB = this.OIDtoMIB(OB);

                                                if (OB == null && e > -1)
                                                {
                                                    OB = SNMP.OID(Value);
                                                    OB = Vendor + OB.Replace(oi, "");
                                                }
                                                if (OB == null)
                                                {
                                                    OB = SNMP.OID(Value);

                                                }
                                                return (OB);
                                            }
                                            else
                                                if (B.SYNTAX.IndexOf("INTEGER") > -1)
                                                {
                                                    if (B.OPTION != null)
                                                    {
                                                        if (B.OPTION.IndexOf(",") > -1)
                                                        {
                                                            try
                                                            {
                                                                int x = B.OPTION.IndexOf("(" + GetU32(Value).ToString());
                                                                String SUB = B.OPTION.Substring(0, x);
                                                                return SUB.Substring(SUB.LastIndexOf(",") + 1).Trim();
                                                            }
                                                            catch
                                                            {
                                                            }
                                                        }
                                                    }
                                                    return (GetU32(Value));
                                                }
                                                else
                                                    if (B.SYNTAX.IndexOf("DateAndTime") > -1)
                                                    {
                                                        Array.Reverse(Value);


                                                        DateTime DT = DateTime.Now;
                                                        try
                                                        {
                                                            DT = new DateTime(BitConverter.ToInt64(Value, 0));
                                                            UInt16 year = BitConverter.ToUInt16(Value, 6); ;
                                                            UInt16 month = Value[5];
                                                            UInt16 day = Value[4];
                                                            UInt16 Hour = Value[3];
                                                            UInt16 Minuts = Value[2];
                                                            UInt16 Seconds = Value[1];
                                                            UInt16 MillSeconds = Value[0];
                                                            DT = new DateTime(year, month, day, Hour, Minuts, Seconds, MillSeconds);
                                                        }
                                                        catch
                                                        {
                                                        }

                                                        return (DT.ToString());
                                                    }
                    }
                    catch
                    {
                    }
                    return GetU32(Value);
                }
                private void AddAtributes(ref XmlTextWriter xtw, ref MIB A)
                {
                    xtw.WriteAttributeString("ACCESS", A.ACCESS);
                    xtw.WriteAttributeString("DEFINITION", A.DEFINITION);
                    xtw.WriteAttributeString("DESCRIPTION", A.DESCRIPTION);
                    xtw.WriteAttributeString("OPTION", A.OPTION);
                    xtw.WriteAttributeString("ParentIndex", A.ParentIndex.ToString());
                    xtw.WriteAttributeString("ParentName", A.ParentName);
                    xtw.WriteAttributeString("SYNTAX", A.SYNTAX);
                    xtw.WriteAttributeString("TYPE", A.TYPE);
                }
                private void FromAttibutes(ref XmlNode Node, ref MIB B, ref MIB A)
                {
                    B.Name = Node.Name;
                    B.ACCESS = Node.Attributes.Item(0).Value;
                    B.DEFINITION = Node.Attributes.Item(1).Value;
                    B.DESCRIPTION = Node.Attributes.Item(2).Value;
                    B.OPTION = Node.Attributes.Item(3).Value;
                    B.ParentIndex = int.Parse(Node.Attributes.Item(4).Value);
                    B.ParentName = Node.Attributes.Item(5).Value;
                    B.SYNTAX = Node.Attributes.Item(6).Value;
                    B.TYPE = Node.Attributes.Item(7).Value;
                    this.AddChild(ref B, ref A);//Add OB and Ref parent node to B
                    B = new MIB();
                }
                /// <summary>
                /// Saves the current MIB base to a XML file;
                /// </summary>
                public void SaveToXml(String FileName)
                {
                    MIB A = null;
                    MIB B = null;
                    A = this;
                    XmlTextWriter xtw = new XmlTextWriter(FileName, System.Text.Encoding.UTF8);

                    //xtw.Formatting = Formatting.Indented;
                    //xtw.Indentation = 3;
                    //xtw.IndentChar = ' ';
                    xtw.WriteStartDocument(true);
                    xtw.WriteStartElement("app");
                    xtw.WriteStartElement("iso");
                    AddAtributes(ref xtw, ref A);
                    #region OIDS
                    for (int X = 0; X <= A.OIDS.Length; X++)
                    {
                        while (A.OIDS.Length == X)//if end of oids go to parent
                        {
                            X = A.ParentIndex + 1;
                            A = A.Parent;
                            xtw.WriteEndElement();//close element
                            if (A == null) { break; }//end of parents
                        }
                        if (A == null) { break; }//end of the oids
                        if (A.OIDS[X] != null)//
                        {

                            A = A.OIDS[X];//go down the chain
                            X = 0;
                            xtw.WriteStartElement(A.Name);
                            AddAtributes(ref xtw, ref A);
                        }
                    }
                    #endregion
                    A = this;
                    B = null;
                    #region RFCs
                    xtw.WriteStartElement("rfc");
                    AddAtributes(ref xtw, ref A);
                    for (int X = 0; X <= A.RFCS.Length; X++)
                    {
                        while (A.RFCS.Length == X)//if end of oids go to parent
                        {
                            X = A.ParentIndex;
                            A = A.Parent;
                            xtw.WriteEndElement();//close element
                            if (A == null) { break; }//end of parents
                            if (A.Name == "iso")
                            {
                                X++;
                            }
                        }
                        if (A == null) { break; }//end of the oids
                        if (X < A.RFCS.Length)//
                        {
                            A = A.RFCS[X];
                            X = -1;
                            xtw.WriteStartElement(A.Name);
                            AddAtributes(ref xtw, ref A);
                        }


                    }
                    #endregion
                    xtw.WriteEndElement();//app
                    xtw.WriteEndDocument();
                    xtw.Close();


                }
                public void SaveToXmlTreeView(String FileName)
                {
                    MIB A = null;
                    MIB B = null;
                    A = this;
                    XmlTextWriter xtw = new XmlTextWriter(FileName, System.Text.Encoding.UTF8);

                    //xtw.Formatting = Formatting.Indented;
                    //xtw.Indentation = 3;
                    //xtw.IndentChar = ' ';
                    xtw.WriteStartDocument(true);
                    xtw.WriteStartElement("TreeNode");
                    //xtw.WriteAttributeString("ImageToolTip", "(" + A.ParentIndex + ")");
                    xtw.WriteAttributeString("Text", A.Name);
                    xtw.WriteAttributeString("ToolTip", A.DESCRIPTION);
                    //xtw.WriteStartElement("iso");
                    //AddAtributes(ref xtw, ref A);
                    #region OIDS
                    for (int X = 0; X <= A.OIDS.Length; X++)
                    {
                        while (A.OIDS.Length == X)//if end of oids go to parent
                        {
                            X = A.ParentIndex + 1;
                            A = A.Parent;
                            xtw.WriteEndElement();//close element
                            if (A == null) { break; }//end of parents
                        }
                        if (A == null) { break; }//end of the oids
                        if (A.OIDS[X] != null)//
                        {

                            A = A.OIDS[X];//go down the chain
                            X = 0;
                            //xtw.WriteStartElement(A.Name);
                            xtw.WriteStartElement("TreeNode");
                            //xtw.WriteAttributeString("ImageToolTip", "(" + A.ParentIndex + ")");
                            xtw.WriteAttributeString("Text", A.Name);
                            xtw.WriteAttributeString("ToolTip", A.DESCRIPTION);
                            //xtw.WriteAttributeString("ImageToolTip", A.DESCRIPTION);
                            // xtw.WriteAttributeString("NavigateUrl", A.DESCRIPTION);
                            //NavigateUrl

                            //AddAtributes(ref xtw, ref A);
                        }
                    }
                    #endregion
                    A = this;
                    B = null;

                    //xtw.WriteEndElement();//app
                    xtw.WriteEndDocument();
                    xtw.Close();

                }
                /// <summary>
                /// Loads the current MIB base from a XML file;
                /// </summary>
                public void LoadFromXml(String FileName)
                {

                    XmlDocument XML = new XmlDocument();
                    MIB A = this;
                    MIB B = new MIB();
                    try
                    {
                        A.ParentIndex = 1;
                        A.Name = "iso";
                        XML.Load(FileName);
                        XmlElement app = XML.DocumentElement;
                        XmlElement iso = (XmlElement)app.ChildNodes[0];
                        XmlElement rfc = (XmlElement)app.ChildNodes[1];
                        XmlNode Node = app.ChildNodes[0];
                        if (iso.Name != "iso" || rfc.Name != "rfc")
                        { throw new System.Exception(); }
                        for (int x = 0; x <= Node.ChildNodes.Count; x++)
                        {
                            while (Node.ChildNodes.Count == x && Node.Name != "app")//if end of ChildNodes go to parent
                            {
                                if (Node.NextSibling == null)
                                {
                                    if (Node.ParentNode.NextSibling == null)
                                    {
                                        while (Node.ParentNode.NextSibling == null && Node.Name != "app")
                                        {
                                            Node = Node.ParentNode;
                                        }
                                        if (Node.Name == "app")
                                        {
                                            break;
                                        }
                                        Node = Node.ParentNode.NextSibling;
                                        if (Node.Name != "rfc")
                                        {
                                            FromAttibutes(ref Node, ref B, ref A);
                                        }
                                        else
                                        {
                                        }
                                    }
                                    else
                                    {
                                        Node = Node.ParentNode.NextSibling;
                                        FromAttibutes(ref Node, ref B, ref A);
                                    }
                                }
                                else
                                {
                                    Node = Node.NextSibling;
                                    FromAttibutes(ref Node, ref B, ref A);
                                }
                            }
                            if (Node.ChildNodes[x] != null && Node.Name != "app")
                            {
                                Node = Node.ChildNodes[x];
                                x = -1;
                                FromAttibutes(ref Node, ref B, ref A);
                            }
                        }
                        MIBParser.MIBS = this;
                    }
                    catch
                    {
                        throw new System.Exception("Xml file does not contain MIB information.");
                    }

                }
            }


            /// <summary>
            /// Base class for Keyed Object Arrays.
            /// </summary>
            public class ArrayLists
            {
                public class Lists
                {
                    public ArrayList Keys = new ArrayList();
                    public ArrayList Values = new ArrayList();
                    public void Sort()
                    {
                        Lists NewList = new Lists();
                        NewList.Keys = (ArrayList)Keys.Clone();
                        NewList.Values = (ArrayList)Values.Clone();



                        NewList.Keys.Sort();
                        for (int X = 0; X < NewList.Keys.Count; X++)
                        {
                            int N = Keys.IndexOf(NewList.Keys[X]);
                            NewList.Values[X] = Values[N];
                        }
                        Values = (ArrayList)NewList.Values.Clone();
                        Keys = (ArrayList)NewList.Keys.Clone();

                    }
                    public bool OnlyAdd(Object Key, Object Value)
                    {

                        if (!Keys.Contains(Key))
                        {
                            Keys.Add(Key);
                            Values.Add(Value);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    public bool Add(Object Key, Object Value)
                    {

                        if (!Keys.Contains(Key))
                        {
                            Keys.Add(Key);
                            Values.Add(Value);
                            return true;
                        }
                        else
                        {
                            Update(Key, Value);
                            return false;
                            //throw new System.Exception(); 
                        }
                    }
                    public void Update(Object Key, Object Value)
                    {
                        int X = Keys.IndexOf(Key);
                        if (X > -1)
                        {

                            Values[X] = Value;
                        }
                        else
                        {
                            throw new System.Exception();
                        }
                    }
                    public int Count
                    {
                        get { return Keys.Count; }
                    }
                    public bool TryGetValue(object Key, out object Value)
                    {
                        int X = Keys.IndexOf(Key);
                        if (X > -1)
                        {
                            Value = Values[X];
                            return true;
                        }
                        Value = null;
                        return false;
                    }
                }
            }


            /// <summary>
            /// Base class for a network device.
            /// </summary>
            public class Device
            {
                public IPAddress IPAddress;
                public int Port = 0;
                public bool SNMP;
                public String TYPE = "UNKNOWN";
                public String _SysDescr = "";      //1.3.6.1.2.1.1.1.0
                public String _SysName = "";       //1.3.6.1.2.1.1.5.0
                public String _SysMac = "";        //1.3.6.1.2.1.2.2.1.6.1
                public DataSet DS;
                public bool ICMP;
                public bool NETB;
                public bool AGNT;
                public bool FOUND = false;
                public Agent Agnt = new Agent();
                /// <summary>
                /// An arrray to store network information.
                /// </summary>
                public ArrayLists.Lists OIDS = new ArrayLists.Lists();
                public void SetAgent()
                {
                    Agnt.IP = IPAddress;
                    Agnt.Port = Port;
                    AGNT = Agnt.Agent_Scan();
                }
            }
            public class SNMP
            {
                public class _Object
                {
                    public _Type Packet_Type;
                    public int Packet_Length;
                    public _Type Version_Type;
                    public int Version_Length;
                    public int Version_Value;
                    public _Type Community_Type;
                    public int Community_Length;
                    public Error_Type Error_Status;
                    public Byte[] CS;
                    public Byte[] OI;
                    public _Type Object_Type;
                    public Byte[] OV;
                }
                public enum Error_Type : byte
                {
                    noError = (0),
                    tooBig = (1),
                    noSuchName = (2),
                    badValue = (3),
                    readOnly = (4),
                    genErr = (5)
                }
                public enum _Type : byte // RFC1213 subset of ASN.1
                {
                    EndMarker = 0x00,
                    Boolean = 0x01,
                    Integer = 0x02,
                    BitString = 0x03,  // internally BitSet
                    OctetString = 0x04, // internally string
                    Null = 0x05,
                    ObjectIdentifier = 0x06, // internally uint[]
                    Sequence = 0x30, // Array
                    IPAddress = 0x40, // byte[]
                    Counter32 = 0x41,
                    Gauge32 = 0x42,
                    TimeTicks = 0x43,
                    Opaque = 0x44,
                    NsapAddress = 0x45,
                    Counter64 = 0x46,
                    UInt32 = 0x47,
                    Get = 0xA0,
                    Next = 0xA1,
                    Response = 0xA2,
                    Set = 0xA3,
                    TrapPDUv1 = 0xA4,
                    GetBulkRequest = 0xA5,
                    InformRequest = 0xA6,
                    TrapPDUv2 = 0xA7

                }
                public static byte[] OID(String oid)
                {
                    char[] separator = { '.' };
                    String[] MIB = oid.Split(separator);
                    int[] Oid = new int[MIB.Length];
                    for (int x = 0; x < MIB.Length; x++)
                    {
                        Oid[x] = int.Parse(MIB[x]);
                    }
                    ArrayList tmpAR = new ArrayList();
                    ArrayList AR = new ArrayList();
                    int i;
                    for (i = 0; i < Oid.Length; i++)
                    {
                        Byte[] tmpy = new byte[0];
                        if (i == Oid.Length) { break; }
                        if (i == 0 && Oid[0] == 1 && Oid[1] == 3)
                        {
                            tmpy = new byte[1];
                            tmpy[0] = 43;
                            i++;
                        }
                        else if (Oid[i] < (1 << 7))
                        {
                            tmpy = new byte[1];
                            tmpy[0] = Convert.ToByte(Oid[i]);
                        }
                        else if (Oid[i] < (1 << 14))
                        {
                            tmpy = new byte[2];
                            tmpy[0] = Convert.ToByte(128 + (Oid[i] >> 7));
                            tmpy[1] = Convert.ToByte(Oid[i] & 127);//AND FUNC
                        }
                        else if (Oid[i] < (1 << 21))
                        {
                            tmpy = new byte[3];
                            tmpy[0] = Convert.ToByte(128 + (Oid[i] >> 14));
                            tmpy[1] = Convert.ToByte(128 + ((Oid[i] >> 7) & 127));//AND FUNC
                            tmpy[2] = Convert.ToByte(Oid[i] & 127);//AND FUNC
                        }
                        else if (Oid[i] < (1 << 28))
                        {
                            tmpy = new byte[4];
                            tmpy[0] = Convert.ToByte(128 + (Oid[i] >> 21));
                            tmpy[1] = Convert.ToByte(128 + ((Oid[i] >> 14) & 127));//AND FUNC
                            tmpy[2] = Convert.ToByte(128 + ((Oid[i] >> 7) & 127));//AND FUNC
                            tmpy[3] = Convert.ToByte(Oid[i] & 127);//AND FUNC
                        }
                        else
                        {
                            tmpy = new byte[5];
                            tmpy[0] = Convert.ToByte(128 + (Oid[i] >> 28));
                            tmpy[1] = Convert.ToByte(128 + ((Oid[i] >> 21) & 127));//AND FUNC
                            tmpy[2] = Convert.ToByte(128 + ((Oid[i] >> 14) & 127));//AND FUNC
                            tmpy[3] = Convert.ToByte(128 + ((Oid[i] >> 7) & 127));//AND FUNC
                            tmpy[4] = Convert.ToByte(Oid[i] & 127);//AND FUNC
                        }
                        for (int idx = 0; idx < tmpy.Length; idx++)
                        {
                            AR.Add(tmpy[idx]);
                        }
                    }
                    Byte[] newIOD = new Byte[AR.Count];
                    AR.CopyTo(newIOD);
                    return (newIOD);
                }
                public static String OID(byte[] oid)
                {
                    String tmp = "";
                    for (int i = 0; i < oid.Length; i++)
                    {
                        if (i == 0 && oid[i] == 43)
                        {
                            tmp += "1.3";
                        }
                        else if (oid[i] < (1 << 7))
                        {
                            tmp += "." + oid[i].ToString();
                        }

                        else if (oid[i] >= (1 << 7))
                        {
                            int x = ((oid[i] & 127) * 128);
                            i++;

                            while (oid[i] >= (1 << 7))
                            {
                                x = ((x * 128) + ((oid[i] & 127) * 128));
                                i++;
                            }



                            x += oid[i];
                            tmp += "." + x.ToString();
                        }

                    }

                    return (tmp);
                }
                public static Byte[] encode(String ComunityString, byte[] OID, String ObjectValue, _Type Snmp_Type)
                {
                    // OID = new byte[];
                    // OID ={ 0x2b, 0x06, 0x01, 0x02, 0x01, 0x01, 0x01, 0x00 };
                    Byte[] CS = ASCIIEncoding.ASCII.GetBytes(ComunityString);
                    //Byte[] OV = null;// ASCIIEncoding.ASCII.GetBytes(ObjectValue);
                    //OV = null;
                    Byte[] tmp = new byte[26 + CS.Length + OID.Length];
                    //
                    tmp[0] = 0x30;             //Packet ASN.1 Type = Sequence 
                    tmp[1] = (byte)(tmp.Length - 2);   //Packet Sequence length
                    //version
                    tmp[2] = 0x02;             //Version ASN.1 Type = Integer
                    tmp[3] = 0x01;             //Version Length
                    tmp[4] = 0x00;             //Version Value
                    //Community
                    tmp[5] = 0x04;             //Community ASN.1 Type = OctetString
                    tmp[6] = (byte)CS.Length;        //Community Length
                    Array.Copy(CS, 0, tmp, 7, CS.Length);   //Community Value
                    //PDU
                    tmp[7 + CS.Length] = (byte)Snmp_Type;// 0xa0;             //PDU Type :    Get(0xA0), Next(0xA1), Response(0xA2), Set(0xA3), Trap(0xA4),
                    tmp[8 + CS.Length] = (byte)(OID.Length + 17);             //PDU Length :
                    //PDU REQUEST ID
                    tmp[9 + CS.Length] = 0x02;             //PDU RequestID ASN.1 Type = Integer
                    tmp[10 + CS.Length] = 0x01;             //PDU RequestID Length
                    tmp[11 + CS.Length] = 0x01;             //PDU RequestID Value
                    //PDU ERROR
                    tmp[12 + CS.Length] = 0x02;             //PDU ERROR Status ASN.1 Type = Integer
                    tmp[13 + CS.Length] = 0x01;             //PDU ERROR Status Length
                    tmp[14 + CS.Length] = 0x00;             //PDU ERROR Status Value :  noError(0), tooBig(1), noSuchName(2), badValue(3), readOnly(4) genErr(5) 
                    tmp[15 + CS.Length] = 0x02;             //PDU ERROR ID ASN.1 Type = Integer
                    tmp[16 + CS.Length] = 0x01;             //PDU ERROR ID Length
                    tmp[17 + CS.Length] = 0x00;             //PDU ERROR ID Value
                    //PDU OBJECT
                    tmp[18 + CS.Length] = 0x30;             //PDU OBJECT ASN.1 Type = Sequence
                    tmp[19 + CS.Length] = (byte)(OID.Length + 6);//PDU OBJECT Length

                    tmp[20 + CS.Length] = 0x30;             //PDU OBJECT PACKET ASN.1 Type = Sequence
                    tmp[21 + CS.Length] = (byte)(OID.Length + 4); //PDU OBJECT PACKET Length
                    tmp[22 + CS.Length] = 0x06;             //PDU OBJECT ID ASN.1 Type = ObjectIdentifier
                    tmp[23 + CS.Length] = (byte)OID.Length;       //PDU OBJECT ID Length
                    Array.Copy(OID, 0, tmp,                 //PDU OBJECT ID Value
                            24 + CS.Length,                 //PDU OBJECT ID Value
                            OID.Length);                    //PDU OBJECT ID Value
                    tmp[24 + CS.Length + OID.Length] = 0x05;//PDU OBJECT Var ASN.1 Type = Null
                    tmp[25 + CS.Length + OID.Length] = 0x00;//PDU OBJECT Var Length


                    //tmp[19 + CS.Length + OID.Length] = 0x01;//PDU OBJECT Var Value
                    return tmp;
                }
                public static _Object decode(byte[] MSG)
                {
                    _Object SNMP_OB = new _Object();


                    int i = 0;
                    _Type Packet_Type = (_Type)MSG[i++];    //SNMP ANS.1 Type
                    int Packet_Length = MSG[i++];       //SNMP ANS.1 Length
                    if (Packet_Length == 0x81) { Packet_Length = MSG[i++]; }
                    else if (Packet_Length == 0x82) { Packet_Length = (((int)MSG[i++] * 256) + (int)MSG[i++]); }
                    // Console.WriteLine( Packet_Length);
                    ///////////VERSION
                    _Type Version_Type = (_Type)MSG[i++];   //Version ANS.1 Type
                    int Version_Length = MSG[i++];      //Version Length
                    int Version_Value = MSG[i++];       //Version Value
                    //Console.WriteLine(Version_Type);

                    //////////COMMUNITY
                    _Type Community_Type = (_Type)MSG[i++];   //Community ANS.1 Type
                    int Community_Length = MSG[i++];      //Community Length
                    if (Community_Length == 0x81) { Community_Length = MSG[i++]; }
                    else if (Community_Length == 0x82) { Community_Length = (((int)MSG[i++] * 256) + (int)MSG[i++]); }

                    Byte[] CS = new Byte[Community_Length]; Array.Copy(MSG, i, CS, 0, Community_Length); i += Community_Length;
                    //Console.WriteLine(ASCIIEncoding.ASCII.GetString( CS));
                    SNMP_OB.CS = CS;


                    //////////PDU
                    _Type PDU_Type = (_Type)MSG[i++];   //PDU ASN.1 Type
                    int PDU_Length = 0; PDU_Length = MSG[i++];       //PDU Length
                    if (PDU_Length == 0x81) { PDU_Length = MSG[i++]; }
                    else if (PDU_Length == 0x82) { PDU_Length = (((int)MSG[i++] * 256) + (int)MSG[i++]); }
                    // Console.WriteLine("PDU_Length " + PDU_Length);

                    //////////PDU ID
                    _Type PDU_ID_Type = (_Type)MSG[i++];
                    int PDU_ID_Length = 0; PDU_ID_Length = MSG[i++];
                    byte[] PDU_ID = new byte[PDU_ID_Length];
                    Array.Copy(MSG, i, PDU_ID, 0, PDU_ID_Length); i += PDU_ID_Length;
                    // i += 6;//ID
                    //////////PDU ERRORS
                    _Type PDU_ERRORS_Type = (_Type)MSG[i++];
                    int PDU_ERRORS_Length = 0; PDU_ERRORS_Length = MSG[i++];
                    byte[] PDU_ERRORS = new byte[PDU_ERRORS_Length];
                    Array.Copy(MSG, i, PDU_ERRORS, 0, PDU_ERRORS_Length); i += PDU_ERRORS_Length;
                    SNMP_OB.Error_Status = (Error_Type)PDU_ERRORS[0];
                    // i += 3;//ERRORS
                    if (SNMP_OB.Error_Status != Error_Type.noError)
                    {
                        return SNMP_OB;
                    }
                    //////////PDU ERRORS
                    _Type PDU_ERRORS_IDX_Type = (_Type)MSG[i++];
                    int PDU_ERRORS_IDX_Length = 0; PDU_ERRORS_IDX_Length = MSG[i++];
                    byte[] PDU_ERRORS_IDX = new byte[PDU_ERRORS_IDX_Length];
                    Array.Copy(MSG, i, PDU_ERRORS_IDX, 0, PDU_ERRORS_IDX_Length); i += PDU_ERRORS_IDX_Length;
                    // i += 3;//ERRORS INDEX


                    //////////PDU OBJECT
                    _Type PDU_OBJECT_Type = (_Type)MSG[i++];   //PDU OBJECT ASN.1 Type
                    int PDU_OBJECT_Length = MSG[i++];      //PDU OBJECT Length
                    if (PDU_OBJECT_Length == 0x81) { PDU_OBJECT_Length = MSG[i++]; }
                    else if (PDU_OBJECT_Length == 0x82) { PDU_OBJECT_Length = ((int)MSG[i++] * 256 + (int)MSG[i++]); }
                    //Console.WriteLine(PDU_OBJECT_Length);

                    //////////PDU OBJECT P
                    _Type PDU_OBJECT_P_Type = (_Type)MSG[i++];   //PDU OBJECT ID ASN.1 Type
                    int PDU_OBJECT_P_Length = MSG[i++];      //PDU OBJECT Length
                    if (PDU_OBJECT_P_Length == 0x81) { PDU_OBJECT_P_Length = MSG[i++]; }
                    else if (PDU_OBJECT_P_Length == 0x82) { PDU_OBJECT_P_Length = ((int)MSG[i++] * 256 + (int)MSG[i++]); }
                    // Console.WriteLine(PDU_OBJECT_ID_Length);

                    //////////PDU OBJECT ID
                    _Type PDU_OBJECT_ID_Type = (_Type)MSG[i++];   //PDU OBJECT ID ASN.1 Type
                    int PDU_OBJECT_ID_Length = MSG[i++];      //PDU OBJECT Length
                    if (PDU_OBJECT_ID_Length == 0x81) { PDU_OBJECT_ID_Length = MSG[i++]; }
                    else if (PDU_OBJECT_ID_Length == 0x82) { PDU_OBJECT_ID_Length = ((int)MSG[i++] * 256 + (int)MSG[i++]); }

                    //////////PDU OBJECT ID VAL
                    Byte[] OI = new Byte[PDU_OBJECT_ID_Length];
                    Array.Copy(MSG, i, OI, 0, PDU_OBJECT_ID_Length); i += PDU_OBJECT_ID_Length;
                    //Console.WriteLine(OID(OI));
                    SNMP_OB.OI = OI;
                    //////////PDU OBJECT VAL
                    _Type PDU_OBJECT_VAL_Type = (_Type)MSG[i++];   //PDU OBJECT ID ASN.1 Type
                    int PDU_OBJECT_VAL_Length = MSG[i++];  //PDU OBJECT Length
                    if (PDU_OBJECT_VAL_Length == 0x81) { PDU_OBJECT_VAL_Length = MSG[i++]; }
                    else if (PDU_OBJECT_VAL_Length == 0x82) { PDU_OBJECT_VAL_Length = ((int)MSG[i++] * 256 + (int)MSG[i++]); }


                    Byte[] OV = new Byte[PDU_OBJECT_VAL_Length];
                    int xx = 0;
                    if (PDU_OBJECT_VAL_Type == _Type.Gauge32)
                    {
                        OV = new Byte[PDU_OBJECT_VAL_Length + 1];
                        OV[0] = 0;
                        xx = 1;
                    }
                    Array.Copy(MSG, i, OV, xx, PDU_OBJECT_VAL_Length); i += PDU_OBJECT_VAL_Length;
                    SNMP_OB.Object_Type = PDU_OBJECT_VAL_Type;
                    SNMP_OB.OV = OV;

                    return SNMP_OB;
                }
            }
            public class Icmp
            {
                public static Random rndm = new Random();
                public class IcmpPacket
                {
                    public IcmpType Type;             // type of message
                    public byte Code;          // type of sub code
                    public ushort CheckSum;         // ones complement checksum of struct
                    public ushort Identifier;       // identifier
                    public ushort SequenceNumber;   // sequence number  
                    public byte[] Data;             // byte array of data
                }
                public enum IcmpType : byte
                {
                    ECHO_REPLY = 0,
                    Destination_Unreachable = 3,
                    Source_Quench = 4,
                    Redirect = 5,
                    ECHO_REQ = 8,
                    TIME_TO_LIVE_EXCEEDED = 11,
                    Parameter_Problem = 12,
                    Timestamp = 13,
                    Timestamp_Reply = 14,
                    Information_Request = 15,
                    Information_Reply = 16,
                    TRACEROUTE = 30,
                    MAX_TTL = 255
                }
                public string PING(IPAddress IP, String Message, int TimeOut, int Retrys)
                {

                    uint iLoop = 0;
                    bool bContinuous = false;
                    bool ALIVE = false;
                    const int MAX_PACKET_SIZE = 65535;
                    int RX_Bytes = 0;
                    int StartTime = 0;
                    int StopTime = 0;
                    long PacketsSent = 0;
                    long PacketsReceived = 0;
                    long TX_Time = 0;
                    int Ping_Count = Retrys;
                    int Count = 0;
                    int Ping_Timeout = TimeOut;
                    int MinTX_Time = int.MaxValue;
                    int MaxTX_Time = int.MinValue;
                    ushort TX_SEQ = 0;


                    byte[] PingBytes = ASCIIEncoding.ASCII.GetBytes(Message);


                    IPEndPoint remotehost = new IPEndPoint(IP, 0);
                    EndPoint remotehostEP = (remotehost);

                    Socket PingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
                    PingSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, Ping_Timeout);
                    PingSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, Ping_Timeout);

                    while (!ALIVE && PacketsSent < Ping_Count)
                    {

                        ushort TX_ID = (ushort)rndm.Next(ushort.MaxValue);
                        byte[] RX_Buff = new byte[MAX_PACKET_SIZE];
                        StartTime = System.Environment.TickCount;
                        PacketsSent++;

                        int PacketSize = PingBytes.Length + 8;
                        byte[] TX_Buff = new byte[PacketSize];
                        if ((PacketSize % 2 == 1))
                        {
                            TX_Buff = new byte[PacketSize + 1];
                            TX_Buff[PacketSize] = 0x21;//"!" added for even datalength
                        }

                        IcmpPacket PingPacket = new IcmpPacket();
                        PingPacket.Type = IcmpType.ECHO_REQ;
                        PingPacket.Code = 0;
                        PingPacket.CheckSum = UInt16.Parse("0");
                        PingPacket.Identifier = TX_ID;
                        PingPacket.SequenceNumber = TX_SEQ++;
                        PingPacket.Data = PingBytes;
                        MakePacket(PingPacket, TX_Buff, PingBytes);
                        PingSocket.SendTo(TX_Buff, remotehostEP);
                        EndPoint localhostEP = PingSocket.LocalEndPoint;


                        try
                        {
                            RX_Bytes = PingSocket.ReceiveFrom(RX_Buff, MAX_PACKET_SIZE, SocketFlags.None, ref remotehostEP);
                            if (remotehostEP.ToString() == remotehost.ToString())
                            {
                                ushort RX_ID = BitConverter.ToUInt16(RX_Buff, 24);
                                ushort RX_SEQ = BitConverter.ToUInt16(RX_Buff, 26);
                                if ((RX_ID - TX_ID) == 0 && (RX_SEQ - (TX_SEQ - 1)) == 0)
                                {
                                    Console.WriteLine(((IcmpType)RX_Buff[20]).ToString());
                                    //return ("ALIVE");
                                    ALIVE = true;
                                    return ("ALIVE");
                                }
                                else
                                {
                                    Console.WriteLine(RX_ID - TX_ID);
                                }
                            }
                            else
                            {
                                Console.WriteLine(remotehostEP.ToString());
                            }

                        }
                        catch //(Exception e)
                        {
                            //Console.WriteLine ("Request timed out. \n{0}", e.Message);
                            // return ("Request timed out.");
                            // bReceived = false;
                            // break;
                        }
                        if (ALIVE) { return ("ALIVE"); } else { return ("Request timed out."); }

                    }

                    return ("ERROR");

                }
                public static void Ping_Packet(ref byte[] TX_Buff, ref ushort TX_ID, String message)
                {

                    const int MAX_PACKET_SIZE = 65535;
                    byte[] PingBytes = ASCIIEncoding.ASCII.GetBytes(message);
                    ushort TX_SEQ = 0;

                    //ushort TX_ID = (ushort)rndm.Next(ushort.MaxValue);
                    TX_ID = (ushort)rndm.Next(ushort.MaxValue);
                    byte[] RX_Buff = new byte[MAX_PACKET_SIZE];
                    int PacketSize = PingBytes.Length + 8;
                    //byte[] TX_Buff = new byte[PacketSize];
                    TX_Buff = new byte[PacketSize];
                    if ((PacketSize % 2 == 1))
                    {
                        TX_Buff = new byte[PacketSize + 1];
                        TX_Buff[PacketSize] = 0x21;//"!" added for even datalength
                    }
                    IcmpPacket PingPacket = new IcmpPacket();
                    PingPacket.Type = IcmpType.ECHO_REQ;
                    PingPacket.Code = 0;
                    PingPacket.CheckSum = UInt16.Parse("0");
                    PingPacket.Identifier = TX_ID;
                    PingPacket.SequenceNumber = TX_SEQ++;
                    PingPacket.Data = PingBytes;
                    MakePacket(PingPacket, TX_Buff, PingBytes);

                }
                public string PING_PACKET(IPAddress IP, String Message, int TimeOut, int Retrys)
                {

                    uint iLoop = 0;
                    bool bContinuous = false;
                    bool ALIVE = false;
                    const int MAX_PACKET_SIZE = 65535;
                    int RX_Bytes = 0;
                    int StartTime = 0;
                    int StopTime = 0;
                    long PacketsSent = 0;
                    long PacketsReceived = 0;
                    long TX_Time = 0;
                    int Ping_Count = Retrys;
                    int Count = 0;
                    int Ping_Timeout = TimeOut;
                    int MinTX_Time = int.MaxValue;
                    int MaxTX_Time = int.MinValue;
                    ushort TX_SEQ = 0;


                    byte[] PingBytes = ASCIIEncoding.ASCII.GetBytes(Message);


                    IPEndPoint remotehost = new IPEndPoint(IP, 0);
                    EndPoint remotehostEP = (remotehost);

                    Socket PingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
                    PingSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, Ping_Timeout);
                    PingSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, Ping_Timeout);

                    while (!ALIVE && PacketsSent < Ping_Count)
                    {

                        ushort TX_ID = (ushort)rndm.Next(ushort.MaxValue);
                        byte[] RX_Buff = new byte[MAX_PACKET_SIZE];
                        StartTime = System.Environment.TickCount;
                        PacketsSent++;

                        int PacketSize = PingBytes.Length + 8;
                        byte[] TX_Buff = new byte[PacketSize];
                        if ((PacketSize % 2 == 1))
                        {
                            TX_Buff = new byte[PacketSize + 1];
                            TX_Buff[PacketSize] = 0x21;//"!" added for even datalength
                        }

                        IcmpPacket PingPacket = new IcmpPacket();
                        PingPacket.Type = IcmpType.ECHO_REQ;
                        PingPacket.Code = 0;
                        PingPacket.CheckSum = UInt16.Parse("0");
                        PingPacket.Identifier = TX_ID;
                        PingPacket.SequenceNumber = TX_SEQ++;
                        PingPacket.Data = PingBytes;
                        MakePacket(PingPacket, TX_Buff, PingBytes);
                        PingSocket.SendTo(TX_Buff, remotehostEP);
                        EndPoint localhostEP = PingSocket.LocalEndPoint;


                        try
                        {
                            RX_Bytes = PingSocket.ReceiveFrom(RX_Buff, MAX_PACKET_SIZE, SocketFlags.None, ref remotehostEP);
                            if (remotehostEP.ToString() == remotehost.ToString())
                            {
                                ushort RX_ID = BitConverter.ToUInt16(RX_Buff, 24);
                                ushort RX_SEQ = BitConverter.ToUInt16(RX_Buff, 26);
                                if ((RX_ID - TX_ID) == 0 && (RX_SEQ - (TX_SEQ - 1)) == 0)
                                {
                                    Console.WriteLine(((IcmpType)RX_Buff[20]).ToString());
                                    //return ("ALIVE");
                                    ALIVE = true;
                                    return ("ALIVE");
                                }
                                else
                                {
                                    Console.WriteLine(RX_ID - TX_ID);
                                }
                            }
                            else
                            {
                                Console.WriteLine(remotehostEP.ToString());
                            }

                        }
                        catch //(Exception e)
                        {
                            //Console.WriteLine ("Request timed out. \n{0}", e.Message);
                            // return ("Request timed out.");
                            // bReceived = false;
                            // break;
                        }
                        if (ALIVE) { return ("ALIVE"); } else { return ("Request timed out."); }

                    }

                    return ("ERROR");

                }
                public static void MakePacket(IcmpPacket Packet, byte[] Buff_8, byte[] PingData)
                {
                    Buff_8[0] = (byte)Packet.Type;
                    Buff_8[1] = Packet.Code;
                    Array.Copy(BitConverter.GetBytes(Packet.CheckSum), 0, Buff_8, 2, 2);
                    Array.Copy(BitConverter.GetBytes(Packet.Identifier), 0, Buff_8, 4, 2);
                    Array.Copy(BitConverter.GetBytes(Packet.SequenceNumber), 0, Buff_8, 6, 2);
                    Array.Copy(PingData, 0, Buff_8, 8, PingData.Length);
                    Array.Copy(BitConverter.GetBytes(CheckSum(Buff_8)), 0, Buff_8, 2, 2);
                }
                /// <summary>
                ///   Checksum - 
                ///     Algorithm to create a checksup for a buffer
                /// </summary>
                public static ushort CheckSum(byte[] Buff_8)
                {
                    int Buff_8_Index = 0;
                    ushort[] Buff_16 = new ushort[Buff_8.Length / 2];
                    for (int i = 0; i < Buff_16.Length; i++)
                    {
                        Buff_16[i] = BitConverter.ToUInt16(Buff_8, Buff_8_Index);
                        Buff_8_Index += 2;
                    }

                    int iCheckSum = 0;
                    for (uint iCount = 0; iCount < Buff_16.Length; iCount++)
                    {
                        iCheckSum += Convert.ToInt32(Buff_16[iCount]);
                    }
                    iCheckSum = (iCheckSum >> 16) + (iCheckSum & 0xffff);
                    iCheckSum += (iCheckSum >> 16);
                    return (ushort)(~iCheckSum);
                }
            }
            public class NetBios
            {
                public String Name_ = "";
                public String Group_ = "";
                //public String Mac_ = "";
                public Byte[] Mac_;
                public Byte[] NameQueryPacket()
                {
                    NCB Ncb = new NCB();
                    //Ncb.TxID = (ushort)rndm.Next(255);
                    Ncb.TxID = 0x0000;
                    Ncb.Flags = 0x0000; //Name query;
                    Ncb.Questions = 0x0001;// 1 Question
                    Ncb.AnswerRRs = 0x0000;
                    Ncb.AuthorityRRs = 0x0000;
                    Ncb.AdditionalRRs = 0x0000;
                    String Workstation = "CK";
                    for (int i = 0; i < 30; i++) { Workstation += "A"; }
                    Byte[] Name = ASCIIEncoding.ASCII.GetBytes(Workstation);
                    Ncb.Queries.Name = new byte[Name.Length + 2];
                    Ncb.Queries.Name[0] = (byte)Name.Length;
                    Array.Copy(Name, 0, Ncb.Queries.Name, 1, Name.Length);
                    Ncb.Queries.Name[Name.Length + 1] = 0x00;
                    Ncb.Queries.Type = (ushort)Type.NBSTAT;
                    Ncb.Queries.Class = (ushort)Class.INET;
                    return encode(Ncb);
                }
                public void NameQuery(String IP)
                {
                    EndPoint Remote = new IPEndPoint(IPAddress.Parse(IP), 137);
                    Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    Sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 500);
                    Sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 500);
                    Sock.SendTo(NameQueryPacket(), Remote);
                    Byte[] Buff = new byte[576];
                    int RxByte = Sock.ReceiveFrom(Buff, ref Remote);
                    //Byte[] RxBuff = new byte[RxByte];
                    NCB ncb = decode(Buff);
                    Name_ = ncb.Answers.Names[0];
                    Group_ = ncb.Answers.Names[1];
                    Mac_ = ncb.Answers.Unit_ID;
                    Sock.Close();
                }
                private Random rndm = new Random();
                public struct NCB
                {
                    //GENERAL FORMAT OF NAME SERVICE PACKETS RFC 1002
                    //HEADER 
                    //
                    //FLAGS
                    /* Optcode Field 5 bits////////////////////////////////////
                     * Symbol   Bit(s)  Desc
                     * 
                     * Optcode    1-4   Operation Specifier:
                     *                      0 = query
                     *                      5 = registration
                     *                      6 = release
                     *                      7 = WACK
                     *                      8 = refresh
                     * R            0   RESPONSE flag:  
                     *                      0 = request
                     *                      1 = response
                     * 
                     * NM_FLAGS Filed 7 bits////////////////////////////////
                     * Symbol   Bit(s)  Desc
                     * 
                     * B            6   Broadcast flag.
                     *                      1 = braodcast or multicast
                     *                      0 = unicast
                     * RA           3   Recusion Available Flag (only in responses from Name Servers all others are 0)
                     * RD           2   Recurion Desired Flag (sent on a request to Name server
                     * TC           1   Truncated Flag >576 use tcp INSTEAD
                     * AA           0   Authoritative Answer flag.Must be zero (0) if R flag of OPCODE is 0 

                     * Reply Code Filed 4 bits////////////////////////////////
                     * Symbol       Value   Desc
                     * 
                     * NO_ERR       0x0     No Error.  
                     * FMT_ERR      0x1     Format Error.
                     * SRV_ERR      0x2     Server failure.
                     * IMP_ERR      0x4     Unsupported request error.
                     * RFS_ERR      0x5     Refused error.
                     * ACT_ERR      0x6     Active error.
                     * CFT_ERR      0x7     Name in conflict error.

                     * */
                    public ushort TxID;//16 bits
                    public ushort Flags;//16 bits see above
                    public ushort Questions;//16 bits number of QUESTION ENTRIES
                    public ushort AnswerRRs;//16 bits number of ANSWER RESOURCE RECORDS
                    public ushort AuthorityRRs;//16 bits number of AUTHORITY RESOURCE RECORDS
                    public ushort AdditionalRRs;//16 bits number of ADDITIONAL RESOURCE RECORDS
                    public Query Queries;
                    public Answer Answers;
                }
                public struct Query
                {
                    public Byte[] Name;
                    public ushort Type;
                    public ushort Class;
                }
                public struct Answer
                {
                    public Byte[] Name;
                    public ushort Type;
                    public ushort Class;
                    public uint TTL;
                    public ushort DataLength;
                    public byte NameCount;
                    public String[] Names;
                    //public String Unit_ID;
                    public Byte[] Unit_ID;
                }
                private enum Type
                {
                    NB = 0x0020,
                    NBSTAT = 0x0021
                }
                private enum Class
                {
                    INET = 0x0001
                }
                public Byte[] encode(NCB ncb)
                {

                    Byte[] tmp = new Byte[16 + ncb.Queries.Name.Length];
                    Array.Copy(LTL_NDN(BitConverter.GetBytes(ncb.TxID)), 0, tmp, 0, 2);
                    Array.Copy(LTL_NDN(BitConverter.GetBytes(ncb.Flags)), 0, tmp, 2, 2);
                    Array.Copy(LTL_NDN(BitConverter.GetBytes(ncb.Questions)), 0, tmp, 4, 2);
                    Array.Copy(LTL_NDN(BitConverter.GetBytes(ncb.AnswerRRs)), 0, tmp, 6, 2);
                    Array.Copy(LTL_NDN(BitConverter.GetBytes(ncb.AuthorityRRs)), 0, tmp, 8, 2);
                    Array.Copy(LTL_NDN(BitConverter.GetBytes(ncb.AdditionalRRs)), 0, tmp, 10, 2);
                    Array.Copy(ncb.Queries.Name, 0, tmp, 12, ncb.Queries.Name.Length);
                    Array.Copy(LTL_NDN(BitConverter.GetBytes(ncb.Queries.Type)), 0, tmp, 12 + ncb.Queries.Name.Length, 2);
                    Array.Copy(LTL_NDN(BitConverter.GetBytes(ncb.Queries.Class)), 0, tmp, 12 + ncb.Queries.Name.Length + 2, 2);
                    return tmp;
                }
                public NCB decode(Byte[] Buff)
                {
                    NCB ncb = new NCB();
                    ncb.TxID = GetU16(Buff, 0);
                    ncb.Flags = GetU16(Buff, 2);
                    ncb.Questions = GetU16(Buff, 4);
                    ncb.AnswerRRs = GetU16(Buff, 6);
                    ncb.AuthorityRRs = GetU16(Buff, 8);
                    ncb.AdditionalRRs = GetU16(Buff, 10);
                    ncb.Answers.Name = new byte[Buff[12] + 2];
                    Array.Copy(Buff, 12, ncb.Answers.Name, 0, ncb.Answers.Name.Length);
                    ncb.Answers.Type = GetU16(Buff, 12 + ncb.Answers.Name.Length);
                    ncb.Answers.Class = GetU16(Buff, 14 + ncb.Answers.Name.Length);
                    ncb.Answers.TTL = GetU32(Buff, 16 + ncb.Answers.Name.Length);
                    ncb.Answers.DataLength = GetU16(Buff, 20 + ncb.Answers.Name.Length);
                    ncb.Answers.NameCount = Buff[22 + ncb.Answers.Name.Length];
                    ncb.Answers.Names = new string[ncb.Answers.NameCount];
                    // ncb.Answers.Names = new string[ncb.Answers.NameCount-1];
                    int IDX1 = 23 + ncb.Answers.Name.Length;
                    for (int i = 0; i < ncb.Answers.Names.Length; i++)
                    {
                        ncb.Answers.Names[i] = ASCIIEncoding.ASCII.GetString(Buff, IDX1, 15);
                        IDX1 += 16 + 2;// 2 = NameFlags
                    }
                    //IDX1 += 16 + 2;
                    Byte[] UID = new Byte[6];
                    if (Buff.Length >= IDX1 + 6)
                    {
                        Array.Copy(Buff, IDX1, UID, 0, 6);
                    }

                    //ncb.Answers.Unit_ID = toHex(UID);
                    ncb.Answers.Unit_ID = UID;


                    return ncb;
                }
                private static String toHex(byte[] b)
                {
                    if (b == null)
                    {
                        return null;
                    }
                    String[] hits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
                    String sb = "";

                    for (int i = 0; i < b.Length; i++)
                    {
                        int j = ((int)b[i]) & 0xFF;
                        sb += (hits[(j / 16)]);
                        sb += (hits[(j % 16)]);
                    }
                    return sb;
                }
                private UInt16 GetU16(Byte[] Buff, int Index)
                {
                    return (BitConverter.ToUInt16(LTL_NDN(BitConverter.GetBytes(BitConverter.ToUInt16(Buff, Index))), 0));
                }
                private UInt32 GetU32(Byte[] Buff, int Index)
                {
                    return (BitConverter.ToUInt32(LTL_NDN(BitConverter.GetBytes(BitConverter.ToUInt32(Buff, Index))), 0));
                }
                private static Byte[] LTL_NDN(Byte[] B) { Array.Reverse(B); return (B); }
            }
            public class Agent
            {
                public String Agent_Status = "System Not Found";

                public IPAddress IP;
                public int Port;

                public bool Agent_Scan()
                {
                    IPEndPoint RemoteEP = new IPEndPoint(IP, Port);
                    Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 500);
                    Sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 500);
                    IAsyncResult result = Sock.BeginConnect(RemoteEP, null, Sock);
                    bool success = result.AsyncWaitHandle.WaitOne(3000, true);
                    bool installed = false;

                    try
                    {

                        if (!success)
                        {
                            Sock.Close();
                            Console.WriteLine(IP.ToString() + "\tConnction Failed");
                            Agent_Status = "Agent Not Found";
                        }
                        else
                        {

                            Sock.EndConnect(result);
                            SendWSize(ref Sock, "<ping/>");
                            string retVal = RecieveWSize(ref Sock);
                            if (retVal == "<pong/>")
                            {
                                installed = true;
                                Agent_Status = "Agent Installed";
                                Console.WriteLine(IP.ToString() + "\t" + "Agent Installed");
                            }
                            else
                            {
                                Agent_Status = "Agent Not Found";
                                Console.WriteLine(IP.ToString() + "\t" + "Agent Not Found");
                            }
                            Sock.Shutdown(SocketShutdown.Both);
                            Sock.Close();
                        }
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine(IP.ToString() + "\t" + ex.Message);
                        {
                            Agent_Status = "Agent Not Found";
                        }
                    }
                    return installed;

                }
                private void SendWSize(ref Socket _Sock, string data)
                {
                    int res = data.Length;
                    byte[] buf = new byte[4];

                    buf[0] = (byte)((res & 0xff000000) >> 24);
                    buf[1] = (byte)((res & 0x00ff0000) >> 16);
                    buf[2] = (byte)((res & 0x0000ff00) >> 8);
                    buf[3] = (byte)((res & 0x000000ff) >> 0);

                    //send the length
                    _Sock.Send(buf);
                    //send the data
                    byte[] SendData = Encoding.ASCII.GetBytes(data);
                    _Sock.Send(SendData);
                }
                private string RecieveWSize(ref Socket _Sock)
                {
                    int BUFSIZE = 0x4000;
                    int res = BUFSIZE;
                    long len = 0;
                    byte[] buf = new byte[BUFSIZE + 1];
                    string str = "";

                    // receive message length
                    res = _Sock.Receive(buf, 4, 0);

                    len = (uint)((buf[0] << 24) & 0xff000000) |
                        (uint)((buf[1] << 16) & 0x00ff0000) |
                        (uint)((buf[2] << 8) & 0x0000ff00) |
                        (uint)((buf[3] << 0) & 0x000000ff);

                    while ((long)(str.Length) < len)
                    {
                        byte[] buffer = new byte[BUFSIZE];

                        res = _Sock.Receive(buffer, BUFSIZE, 0);

                        str += Encoding.ASCII.GetString(buffer, 0, res);
                        buffer = null;
                    }


                    return str;


                }
            }
            public class Scanner
            {
                #region Properties
                private int Delay1 = 0;
                private int Delay2 = 0;
                public int TimeOut = 1000;
                public String XMLReport = "";
                public TimeSpan Time;
                private UInt32 Start;
                private UInt32 Stop;
                private String[] CS;
                private int Port = 0;
                private String sysName = "1.3.6.1.2.1.1.5";//MIB.GetOID("sysName");
                private UInt32 IP_INDEX(IPAddress IP)
                {
                    byte[] A = IP.GetAddressBytes();
                    byte[] A_Tmp = new byte[4];
                    Array.Copy(A, A_Tmp, A.Length);
                    Array.Reverse(A_Tmp);
                    return BitConverter.ToUInt32(A_Tmp, 0);
                }
                private IPAddress IP_INDEX(UInt32 i)
                {
                    byte[] TMP = BitConverter.GetBytes(i);
                    Array.Reverse(TMP);
                    long L = BitConverter.ToUInt32(TMP, 0);
                    return new IPAddress(L);
                }
                public DataTable DeviceTable = new DataTable();
                public MIB MIBS = new MIB();
                #endregion

                #region Base Scans
                String MIBSystem = "sysName,sysDescr,sysContact,sysLocation,sysObjectID,ifNumber,";
                String MIBType = "ipForwarding,prtGeneralConfigChanges,dot1dBaseNumPorts,hrSystemUptime,";
                String MIBInterface = "ifIndex,ifDescr,ifType,ifPhysAddress,ifName,ifMtu,ifSpeed,ifAdminStatus,ifOperStatus,ifLastChange,ifAlias,portName,ipAdEntAddr,ipAdEntIfIndex,ipAdEntNetMask,";
                String MIBHardware = "hrDeviceEntry,";
                String MIBStorage = "hrStorageEntry,";
                String MIBDisks = "hrDiskStorageEntry,";
                String MIBFileSystems = "hrFSEntry,";
                String MIBInstalledSoftware = "hrSWInstalledEntry,";
                String MIBRunningApps = "hrSWRunEntry,";
                String MIBRunningServices = "svSvcEntry,";
                String MIBShares = "svShareEntry,";
                String MIBUserAccounts = "svUserEntry,";
                private String[] mibs;
                #endregion

                #region Lists
                public ArrayLists.Lists OIDS = new ArrayLists.Lists();
                public ArrayLists.Lists HOSTS = new ArrayLists.Lists();
                #endregion

                public enum InventoryScans
                {
                    DeviceDiscovery,//  default
                    Full,
                    Interface,
                    Hardware,
                    Storage,
                    Software,
                    RunningApps,
                    RunningServices,
                    SharedFiles,
                    UserAccount,
                }

                /// <summary>
                /// Supply a start and stop IP to scan, a delay, 
                /// the Diretory where the "RFC_BASE_MINIMUM" folder with the base MIB files exists
                /// and any additional mibs you would like to scan for.
                /// Already includes the System table by default
                /// </summary>
                /// <param name="Start_Host"></param>
                /// <param name="Stop_Host"></param>
                /// <param name="_CS"></param>
                /// <param name="delay"></param>
                /// <param name="dir"></param>
                /// <param name="Scan"></param>
                public Scanner(String Start_Host, String Stop_Host, String _CS, int delay, String dir, InventoryScans[] Scan, bool AgentScan, bool DisplayAll,bool AutoXMLReport)
                {
                    String MIBList = MIBSystem + MIBType;//Manditory
                    for (int X = 0; X < Scan.Length; X++)
                    {
                        switch (Scan[X])
                        {
                            case (InventoryScans.Hardware):
                                MIBList += MIBHardware;
                                break;
                            case (InventoryScans.Interface):
                                MIBList += MIBInterface;
                                break;
                            case (InventoryScans.RunningApps):
                                MIBList += MIBRunningApps;
                                break;
                            case (InventoryScans.RunningServices):
                                MIBList += MIBRunningServices;
                                break;
                            case (InventoryScans.SharedFiles):
                                MIBList += MIBShares;
                                break;
                            case (InventoryScans.Software):
                                MIBList += MIBInstalledSoftware;
                                break;
                            case (InventoryScans.Storage):
                                MIBList += MIBStorage += MIBDisks += MIBFileSystems;
                                break;
                            case (InventoryScans.UserAccount):
                                MIBList += MIBUserAccounts;
                                break;
                            case (InventoryScans.Full):
                                MIBList += MIBHardware;
                                MIBList += MIBInterface;
                                MIBList += MIBRunningApps;
                                MIBList += MIBRunningServices;
                                MIBList += MIBShares;
                                MIBList += MIBInstalledSoftware;
                                MIBList += MIBStorage += MIBDisks += MIBFileSystems;
                                MIBList += MIBUserAccounts;
                                break;
                        }
                        if (Scan[X] == InventoryScans.Full)
                        { break; }
                    }
                    StartScan(Start_Host, Stop_Host, _CS, delay, dir, MIBList, AgentScan, DisplayAll, AutoXMLReport);
                }
                /// <summary>
                /// Supply a start and stop IP to scan, a delay, 
                /// the Diretory where the "RFC_BASE_MINIMUM" folder with the base MIB files exists
                /// and any additional mibs you would like to scan for.
                /// Already includes the System table by default
                /// </summary>
                /// <param name="Start_Host"></param>
                /// <param name="Stop_Host"></param>
                /// <param name="_CS"></param>
                /// <param name="delay"></param>
                /// <param name="dir"></param>
                /// <param name="MIBList"></param>
                public Scanner(String Start_Host, String Stop_Host, String _CS, int delay, String dir, String MIBList, bool AgentScan, bool DisplayAll, bool AutoXMLReport)
                {
                    MIBList = MIBSystem + MIBType + MIBList;//Manditory
                    StartScan(Start_Host, Stop_Host, _CS, delay, dir, MIBList, AgentScan, DisplayAll, AutoXMLReport);
                }
                public Scanner()
                {
                }
                private void StartScan(String Start_Host, String Stop_Host, String _CS, int delay, String dir, String MIBList, bool AgentScan, bool DisplayAll, bool AutoXMLReport)
                {
                    MIBS = new MIB(dir);//DIR WHERE RFC_BASE_MINIMUM// exists
                    mibs = MIBList.Split(",".ToCharArray());
                    RemoveEmpty(ref mibs);
                    Console.WriteLine(MIBS.OIDS.Length);
                    Console.ReadLine();
                    Delay1 = 0;
                    Delay2 = delay;
                    CS = _CS.Split(",".ToCharArray());
                    Start = IP_INDEX(IPAddress.Parse(Start_Host));
                    Stop = IP_INDEX(IPAddress.Parse(Stop_Host));
                    #region Build OIDS and device table
                    BuildOIDS();
                    DeviceTable.Columns.Add("ID");
                    DeviceTable.Columns.Add("SubNet Mask");
                    DeviceTable.Columns.Add("MAC Address");
                    DeviceTable.Columns.Add("Scan Protocols");
                    DeviceTable.Columns.Add("Name");
                    DeviceTable.Columns.Add("Type");
                    DeviceTable.Columns.Add("Object ID");
                    DeviceTable.Columns.Add("Description");
                    DeviceTable.Columns.Add("Contact");
                    DeviceTable.Columns.Add("Location");
                    DeviceTable.Columns.Add("Interface Count");
                    DeviceTable.PrimaryKey = new DataColumn[] { DeviceTable.Columns["ID"] };
                    #endregion
                    DateTime START = DateTime.Now;
                    Object OB = new object();

                    #region ICMP
                    ICMP_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, TimeOut);
                    ICMP_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, int.MaxValue);
                    ICMP_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, int.MaxValue);
                    Thread Thread_ICMP = new Thread(new ThreadStart(ICMP_Receive));
                    Thread_ICMP.Start();
                    for (int Xs = 0; Xs < 2; Xs++)
                    {
                        for (UInt32 i = Start; i <= Stop; i++)
                        {
                            if (!HOSTS.TryGetValue(i, out OB))//look for device in HOSTS List
                            {
                                ICMP_Scan(IP_INDEX(i));
                            }
                        }
                    }
                    Thread_ICMP.Join();
                    #endregion

                    #region Agent
                    Thread[] AgentThreads = new Thread[HOSTS.Values.Count];
                    if (AgentScan)
                    {


                        for (int D = 0; D < HOSTS.Values.Count; D++)
                        {
                            if (((Device)HOSTS.Values[D]).ICMP)
                            {
                                ((Device)HOSTS.Values[D]).Port = 23;//telnet
                                AgentThreads[D] = new Thread(new ThreadStart(((Device)HOSTS.Values[D]).SetAgent));
                                AgentThreads[D].Start();
                            }
                        }

                    }
                    #endregion

                    #region UDP
                    UDP__Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, TimeOut);
                    UDP__Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, int.MaxValue);
                    UDP__Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, int.MaxValue);
                    Thread Thread_SNMP = new Thread(new ThreadStart(UDP__Receive));
                    Thread_SNMP.Start();

                    for (int Xs = 0; Xs < 1; Xs++)
                    {
                        for (UInt32 i = Start; i <= Stop; i++)
                        {
                            if (!HOSTS.TryGetValue(i, out OB))//look for device in HOSTS List
                            {
                            }
                            else
                            {
                                if (!((Device)OB).SNMP)
                                {
                                    SNMP_Scan(IP_INDEX(i));
                                }
                            }
                        }

                    }
                    Thread_SNMP.Join();
                    Thread Thread_NETB = new Thread(new ThreadStart(UDP__Receive));
                    Thread_NETB.Start();
                    for (int Xs = 0; Xs < 1; Xs++)
                    {
                        for (UInt32 i = Start; i <= Stop; i++)
                        {
                            if (!HOSTS.TryGetValue(i, out OB))//look for device in HOSTS List
                            {

                            }
                            else
                            {
                                if (!((Device)OB).NETB)
                                {
                                    NETB_Scan(IP_INDEX(i));
                                }
                            }
                        }
                    }
                    Thread_NETB.Join();
                    #endregion
                    //Agent thread clean up
                    for (int lIdx = 0; lIdx < AgentThreads.Length; lIdx++)
                    {
                        if (AgentThreads[lIdx] != null) { AgentThreads[lIdx].Join(); }
                    }
                    if (DisplayAll)
                    {
                        Device DeviceNotFound = new Device();
                        for (UInt32 i = Start; i <= Stop; i++)
                        {
                            if (!HOSTS.TryGetValue(i, out OB))//look for device in HOSTS List
                            {
                                DeviceNotFound = new Device();
                                DeviceNotFound.IPAddress = IP_INDEX(i);
                                HOSTS.Add(i, DeviceNotFound);
                            }
                        }
                    }


                    HOSTS.Sort();
                    Compile();
                    BuildTables();

                    if (AutoXMLReport)
                    {
                        String FileName = Start_Host + "-" + Stop_Host + "-" + DateTime.Now;
                        XMLReport = FileName.Replace("/", "-").Replace(":", ".") + ".xml";
                        HOSTStoXML(dir + XMLReport);
                    }
                   //HOSTStoSQL();

                    BuildCustomTables();
                    DateTime STOP = DateTime.Now;
                    Time = STOP - START;
                }

                #region Scanning
                private void populate_device(EndPoint RemoteEP, Byte[] Data)
                {
                    String EP = RemoteEP.ToString();
                    bool Walk = false;
                    IPAddress IP = IPAddress.Parse(EP.Remove(EP.IndexOf(":"), (EP.Length - EP.IndexOf(":"))));
                    int port = int.Parse(EP.Substring(EP.IndexOf(":") + 1));
                    UInt32 IDX = IP_INDEX(IP);
                    if (IDX >= Start && IDX <= Stop)
                    {
                        Device dev = new Device();
                        Object OB = new object();
                        dev.IPAddress = IP;
                        #region Find or Add Device
                        if (!HOSTS.TryGetValue(IDX, out OB))//look for device in HOSTS List
                        {
                            dev = new Device();
                            dev.IPAddress = IP;
                            HOSTS.Add(IDX, dev);
                            HOSTS.TryGetValue(IDX, out OB);
                        }
                        #endregion
                        #region IF SNMP
                        if (port == 161)
                        {
                            dev = (Device)OB;
                            dev.FOUND = true;
                            dev.SNMP = true;
                            SNMP._Object SNMP_OB = SNMP.decode(Data);
                            if (SNMP_OB.Error_Status == SNMP.Error_Type.noError)
                            {
                                String _OID = "", cs = "", OID = "", _oid = "", _mib = "";
                                _OID = SNMP.OID(SNMP_OB.OI);
                                cs = ASCIIEncoding.ASCII.GetString(SNMP_OB.CS);
                                if (OID_FOUND(_OID, out OID, out _oid, out _mib))
                                {
                                    Walk = dev.OIDS.Add(_OID, SNMP_OB.OV);
                                    if (_mib == "sysName")
                                    {
                                        String Sysname = ASCIIEncoding.ASCII.GetString(SNMP_OB.OV);
                                        if (dev._SysName != null && dev._SysName != Sysname)
                                        {
                                            for (int i = 0; i < OIDS.Values.Count; i++)//SCAN ALL MIBS 
                                            {
                                                if (OIDS.Values[i].ToString() != "sysName")
                                                {
                                                    Thread.Sleep(Delay1);
                                                    Thread.SpinWait(Delay2 * 10);
                                                    UDP__Socket.SendTo(SNMP.encode(cs, SNMP.OID(OIDS.Keys[i].ToString()), null, SNMP._Type.Next), RemoteEP);
                                                }
                                            }
                                        }
                                    }
                                    else if (Walk && _oid != "0")//GET NEXT ONLY IF MORE THAN 0 IN CURRENT OID
                                    {


                                        Thread.Sleep(Delay1);
                                        Thread.SpinWait(Delay2);
                                        UDP__Socket.SendTo(SNMP.encode(cs, SNMP_OB.OI, null, SNMP._Type.Next), RemoteEP);
                                    }

                                }
                            }
                        }
                        #endregion
                        #region IF NBIOS
                        else if (port == 137)
                        {
                            dev = (Device)OB;
                            dev.FOUND = true;
                            NetBios.NCB ncb = new NetBios().decode(Data);
                            dev.NETB = true;
                            if (dev.TYPE == "UNKNOWN") { dev.TYPE = "HOST"; }
                            String[] NETB_IOD = { 
                                MIBS.MIBtoOID("sysName","system") + ".0", 
                                MIBS.MIBtoOID("sysDescr","system") + ".0",
                                MIBS.MIBtoOID("ifPhysAddress","ifEntry") + ".1",
                                MIBS.MIBtoOID("ipAdEntIfIndex","ipAddrEntry") + "." + IP.ToString() 
                            };
                            object temp;
                            byte[] x = { 1 };

                            for (int ii = 0; ii < NETB_IOD.Length; ii++)
                            {
                                if (!dev.OIDS.TryGetValue(NETB_IOD[ii], out temp))//if device found look for OID
                                {
                                    if (ncb.Answers.Names.Length > 0)
                                    {
                                        if (ii == 0) { temp = ASCIIEncoding.ASCII.GetBytes(ncb.Answers.Names[0]);}
                                        if (ncb.Answers.Names.Length > 1)
                                        {
                                            if (ii == 1) { temp = ASCIIEncoding.ASCII.GetBytes(ncb.Answers.Names[1]); }
                                        }
                                        if (ii == 2) { temp = ncb.Answers.Unit_ID; }
                                        if (ii == 3) { temp = x; }
                                        if (!dev.SNMP)
                                        {
                                            dev.OIDS.OnlyAdd(NETB_IOD[ii], temp);//if OID is not there add it and the DATA
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region IF ICMP
                        else if (port == 0)
                        {
                            dev = (Device)OB;
                            dev.FOUND = true;
                            dev.ICMP = true;

                        }
                        #endregion

                        HOSTS.Add(IDX, dev); ;
                    }
                }
                #region UDP
                private Socket UDP__Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                private void UDP__Receive()
                {
                    EndPoint RemoteEP = new IPEndPoint(IPAddress.Any, 0);
                    UDP__Socket.SendTo(new byte[0], new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
                    byte[] ByteBuffer = new byte[65535];
                    while (true)
                    {
                        try
                        {

                            int Received_Count = 0;
                            Received_Count = UDP__Socket.ReceiveFrom(ByteBuffer, ref RemoteEP);
                            byte[] ByteData = new byte[Received_Count];
                            Array.Copy(ByteBuffer, ByteData, Received_Count);
                            populate_device(RemoteEP, ByteData);


                        }
                        catch (Exception e)
                        {
                            if (e.GetType() == typeof(SocketException))
                            {
                                if (((SocketException)e).ErrorCode == 10054)
                                {
                                }
                                else if (((SocketException)e).ErrorCode == 10060)
                                {
                                    break;
                                }
                                else
                                {
                                    throw e;
                                }
                            }
                            else
                            {
                                throw e;
                            }
                        }


                    }

                }
                private void SNMP_Scan(IPAddress IP)
                {
                    Thread.Sleep(Delay1);
                    Thread.SpinWait(Delay2 * 10);
                    if (IP.GetAddressBytes()[3] > 0)
                    {
                        for (int i = 0; i < CS.Length; i++)
                        {
                            UDP__Socket.SendTo(SNMP.encode(CS[i], SNMP.OID(sysName), null, SNMP._Type.Next), new IPEndPoint(IP, 161));
                        }
                    }
                }
                private void NETB_Scan(IPAddress IP)
                {
                    Thread.Sleep(Delay1);
                    Thread.SpinWait(Delay2);
                    if (IP.GetAddressBytes()[3] > 0)
                    {
                        UDP__Socket.SendTo(new NetBios().NameQueryPacket(), new IPEndPoint(IP, 137));
                    }
                }
                #endregion
                #region ICMP
                private Socket ICMP_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
                private void ICMP_Receive()
                {
                    ICMP_Socket.SendTo(new byte[0], new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
                    EndPoint RemoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] ByteBuffer = new byte[65535];
                    while (true)
                    {
                        try
                        {
                            Thread.Sleep(Delay1);
                            Thread.SpinWait(Delay2);
                            int Received_Count = ICMP_Socket.ReceiveFrom(ByteBuffer, ref RemoteEP);
                            byte[] ByteData = new byte[Received_Count];
                            Array.Copy(ByteBuffer, ByteData, Received_Count);
                            ICMP_Parse(RemoteEP, ByteData);
                        }
                        catch (Exception e)
                        {
                            if (e.GetType() == typeof(SocketException))
                            {
                                if (((SocketException)e).ErrorCode == 10054)
                                {
                                }
                                else if (((SocketException)e).ErrorCode == 10060)
                                {
                                    break;
                                }
                                else
                                {
                                    throw e;
                                }
                            }
                            else
                            {
                                //throw e;
                            }
                        }
                    }

                }
                private void ICMP_Scan(IPAddress IP)
                {
                    Thread.Sleep(Delay1);
                    Thread.SpinWait(Delay2);
                    if (IP.GetAddressBytes()[3] > 0)
                    {
                        byte[] TX_Buff = new byte[0];
                        ushort TX_ID = 0;
                        Icmp.Ping_Packet(ref TX_Buff, ref TX_ID, IP.ToString());
                        ICMP_Socket.SendTo(TX_Buff, new IPEndPoint(IP, 161));
                    }
                }
                private void ICMP_Parse(EndPoint RemoteEP, Byte[] Data)
                {
                    String R_HOST_CHECK = ASCIIEncoding.ASCII.GetString(Data, 28, Data.Length - 28).Replace("!", "");
                    if (RemoteEP.ToString() == R_HOST_CHECK + ":0")
                    {
                        populate_device(RemoteEP, Data);

                    }
                }
                #endregion
                #endregion

                #region Compile & Build Tables
                private void BuildOIDS()
                {
                    for (int i = 0; i < mibs.Length; i++)
                    {
                        String[] O = MIBS.GetOIDS(mibs[i], true);
                        for (int fo = 0; fo < O.Length; fo++)
                        {
                            OIDS.Add(O[fo], mibs[i]);
                            Console.WriteLine(mibs[i]);
                        }

                    }
                }
                private bool OID_FOUND(String _OID, out String OID, out String _oid, out String _mib)
                {
                    String[] A = _OID.Split(".".ToCharArray());
                    String[] B = null;
                    bool same = false;
                    OID = null;
                    _oid = null;
                    _mib = null;
                    for (int X = 0; X < OIDS.Keys.Count; X++)
                    {
                        B = OIDS.Keys[X].ToString().Split(".".ToCharArray());
                        if (B.Length < A.Length)
                        {
                            int q;
                            for (q = 0; q < B.Length; q++)
                            {
                                if (B[q] == A[q]) { same = true; } else { same = false; break; }
                            }
                            if (same)
                            {
                                OID = OIDS.Keys[X].ToString();
                                _oid = _OID.Replace(OID + ".", "");
                                _mib = OIDS.Values[X].ToString();
                                return same;
                            }
                        }
                    }

                    return false;
                }
                private void Compile()
                {
                    Device D;
                    MIB B = new MIB();
                    Object O;
                    for (int x = 0; x < HOSTS.Values.Count; x++)
                    {
                        D = (Device)HOSTS.Values[x];
                        if (D.ICMP)
                        {
                            for (int i = 0; i < D.OIDS.Keys.Count; i++)
                            {
                                MIBS.OIDtoMIB(D.OIDS.Keys[i].ToString(), ref B);
                                O = MIBS.ProcessObject(B, (Byte[])D.OIDS.Values[i]);
                                D.OIDS.Values[i] = O;
                                if (D.TYPE == "UNKNOWN" || D.TYPE == "SWITCH")
                                {
                                    if (B.Name == "ipForwarding" && O.ToString() == "forwarding") { D.TYPE = "ROUTER"; }
                                    if (B.Name == "prtGeneralConfigChanges" && O.ToString().Length > 0) { D.TYPE = "PRINTER"; }
                                    if (B.Name == "dot1dBaseNumPorts" && int.Parse(O.ToString()) > 0) { D.TYPE = "SWITCH"; }
                                    if (B.Name == "hrSystemUptime" && O.ToString().Length > 0) { D.TYPE = "HOST"; }
                                }
                            }
                        }
                    }
                }
                private void BuildTables()
                {
                    Device Dev = null;
                    MIB B = null;
                    String mib = "";
                    String _OID = "";
                    String OID = "";
                    String _oid = "";
                    DataRow DR;
                    DataView DV;

                    for (int i = 0; i < HOSTS.Values.Count; i++)
                    {
                        Dev = (Device)HOSTS.Values[i];
                        Dev.DS = new DataSet("Report");
                        for (int x = 0; x < Dev.OIDS.Count; x++)
                        {
                            _OID = Dev.OIDS.Keys[x].ToString();
                            if (MIBS.OID_FOUND(_OID, out OID, out _oid, ref B) && Dev.ICMP)
                            {
                                if (!Dev.DS.Tables.Contains(B.ParentName))//Tables
                                {
                                    Dev.DS.Tables.Add(B.ParentName);
                                    Dev.DS.Tables[B.ParentName].Columns.Add("ID");
                                    Dev.DS.Tables[B.ParentName].PrimaryKey = new DataColumn[] { Dev.DS.Tables[B.ParentName].Columns["ID"] };
                                    DR = Dev.DS.Tables[B.ParentName].NewRow();
                                    DR["ID"] = _oid;
                                    Dev.DS.Tables[B.ParentName].Rows.Add(DR);
                                }
                                if (!Dev.DS.Tables[B.ParentName].Columns.Contains(B.Name))//Columns
                                {
                                    Dev.DS.Tables[B.ParentName].Columns.Add(B.Name);
                                }
                                if (!Dev.DS.Tables[B.ParentName].Rows.Contains(_oid))//Rows
                                {
                                    DR = Dev.DS.Tables[B.ParentName].NewRow();
                                    DR["ID"] = _oid;
                                    Dev.DS.Tables[B.ParentName].Rows.Add(DR);
                                }
                                Dev.DS.Tables[B.ParentName].Rows.Find(_oid)[B.Name] = Dev.OIDS.Values[x];
                            }

                        }
                    }


                }
                private void BuildCustomTables()
                {
                    Device Dev = null;
                    for (int i = 0; i < HOSTS.Values.Count; i++)
                    {
                        Dev = (Device)HOSTS.Values[i];
                        AddToDEEVICETable(Dev);
                        HOSTS.Values[i] = Dev;
                    }
                }
                private void HOSTStoXML(String FileName)
                {
                    XmlTextWriter xtw = new XmlTextWriter(FileName, System.Text.Encoding.Default);
                    xtw.Formatting = Formatting.Indented;
                    xtw.Indentation = 2;
                    xtw.IndentChar = ' ';
                    xtw.WriteStartDocument(true);
                    xtw.WriteStartElement("HOSTS");
                    Device Dev = null;
                    for (int i = 0; i < HOSTS.Values.Count; i++)
                    {
                        Dev = (Device)HOSTS.Values[i];
                        String sysName = "";
                        String sysDescr = "";
                        String sysContact = "";
                        String sysLocation = "";
                        String sysObjectID = "";
                        String ifNumber = "";
                        String ifPhysAddress = "";
                        String ipAdEntNetMask = "";
                        String ifIndex = "";
                        String Scan_Protocols = "";
                        if (Dev.ICMP) { Scan_Protocols = "Icmp/"; }
                        if (Dev.NETB) { Scan_Protocols += "NetBios/"; }
                        if (Dev.SNMP) { Scan_Protocols += "Snmp/"; }
                        if (Dev.AGNT) { Scan_Protocols += "Agent/"; }
                        Object Val = null;
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("sysName", "system") + ".0", out Val)) { sysName = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("sysDescr", "system") + ".0", out Val)) { sysDescr = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("sysContact", "system") + ".0", out Val)) { sysContact = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("sysLocation", "system") + ".0", out Val)) { sysLocation = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("sysObjectID", "system") + ".0", out Val)) { sysObjectID = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("ifNumber", "interfaces") + ".0", out Val)) { ifNumber = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("ipAdEntIfIndex", "ipAddrEntry") + "." + Dev.IPAddress.ToString(), out Val)) { ifIndex = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("ipAdEntNetMask", "ipAddrEntry") + "." + Dev.IPAddress.ToString(), out Val)) { ipAdEntNetMask = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("ifPhysAddress", "ifEntry") + "." + ifIndex, out Val)) { ifPhysAddress = Val.ToString(); }
                        xtw.WriteStartElement("Device");
                        xtw.WriteElementString("PrimaryKey", IP_INDEX(Dev.IPAddress).ToString());
                        xtw.WriteElementString("ID", Dev.IPAddress.ToString());
                        xtw.WriteElementString("SubNet_Mask", ipAdEntNetMask);
                        xtw.WriteElementString("MAC_Address", ifPhysAddress);
                        xtw.WriteElementString("Scan_Protocols", Scan_Protocols);
                        xtw.WriteElementString("Name", sysName.Replace(" ",""));
                        xtw.WriteElementString("Type", Dev.TYPE.Trim());
                        xtw.WriteElementString("Object_ID", sysObjectID.Trim());
                        xtw.WriteElementString("Description", sysDescr.Trim());
                        xtw.WriteElementString("Contact", sysContact.Trim());
                        xtw.WriteElementString("Location", sysLocation.Trim());
                        xtw.WriteElementString("Interface_Count", ifNumber.Trim());

                        for (int dt = 0; dt < Dev.DS.Tables.Count; dt++)
                        {
                            for (int dr = 0; dr < Dev.DS.Tables[dt].Rows.Count; dr++)
                            {
                                xtw.WriteStartElement(Dev.DS.Tables[dt].TableName);
                                xtw.WriteElementString("PrimaryKey", IP_INDEX(Dev.IPAddress).ToString());
                                for (int dc = 0; dc < Dev.DS.Tables[dt].Columns.Count; dc++)
                                {
                                    xtw.WriteElementString(Dev.DS.Tables[dt].Columns[dc].ColumnName, Dev.DS.Tables[dt].Rows[dr][dc].ToString());
                                }
                                xtw.WriteEndElement();//DataColumn
                            }
                        }

                        xtw.WriteEndElement();//Device


                    }
                    xtw.WriteEndElement();//HOSTS
                    xtw.WriteEndDocument();
                    xtw.Close();
                }
                private void AddToDEEVICETable(Device Dev)
                {
                    DataRow DR;
                    if (!DeviceTable.Rows.Contains(Dev.IPAddress))//Rows
                    {
                        DR = DeviceTable.NewRow();
                        DR["ID"] = Dev.IPAddress;
                        DeviceTable.Rows.Add(DR);
                        DR["Type"] = Dev.TYPE;
                        if (Dev.ICMP)
                        {
                            DR["Scan Protocols"] = "Icmp/";
                        }
                        if (Dev.NETB)
                        {
                            DR["Scan Protocols"] += "NetBios/";
                        }
                        if (Dev.SNMP)
                        {
                            DR["Scan Protocols"] += "Snmp/";
                        }
                        if (Dev.AGNT)
                        {
                            DR["Scan Protocols"] += "Agent/";
                        }
                    }
                    if (Dev.DS.Tables.Contains("system"))
                    {
                        if (Dev.DS.Tables["system"].Columns.Contains("sysName"))
                        {
                            DeviceTable.Rows.Find(Dev.IPAddress)["Name"] = Dev.DS.Tables["system"].Rows[0]["sysName"];
                        }
                        if (Dev.DS.Tables["system"].Columns.Contains("sysDescr"))
                        {
                            DeviceTable.Rows.Find(Dev.IPAddress)["Description"] = Dev.DS.Tables["system"].Rows[0]["sysDescr"];
                        }
                        if (Dev.DS.Tables["system"].Columns.Contains("sysContact"))
                        {
                            DeviceTable.Rows.Find(Dev.IPAddress)["Contact"] = Dev.DS.Tables["system"].Rows[0]["sysContact"];
                        }
                        if (Dev.DS.Tables["system"].Columns.Contains("sysLocation"))
                        {
                            DeviceTable.Rows.Find(Dev.IPAddress)["Location"] = Dev.DS.Tables["system"].Rows[0]["sysLocation"];
                        }
                        if (Dev.DS.Tables["system"].Columns.Contains("sysObjectID"))
                        {
                            DeviceTable.Rows.Find(Dev.IPAddress)["Object ID"] = Dev.DS.Tables["system"].Rows[0]["sysObjectID"];
                        }
                    }
                    if (Dev.DS.Tables.Contains("interfaces"))
                    {
                        if (Dev.DS.Tables["interfaces"].Columns.Contains("ifNumber"))
                        {
                            DeviceTable.Rows.Find(Dev.IPAddress)["Interface Count"] = Dev.DS.Tables["interfaces"].Rows[0]["ifNumber"];
                        }
                    }
                    try
                    {
                        if (Dev.DS.Tables.Contains("ifEntry") && Dev.DS.Tables.Contains("ipAddrEntry"))
                        {

                            Object ID = Dev.DS.Tables["ipAddrEntry"].Rows.Find(Dev.IPAddress)["ipAdEntIfIndex"];
                            DeviceTable.Rows.Find(Dev.IPAddress)["MAC Address"] = Dev.DS.Tables["ifEntry"].Rows.Find(ID)["ifPhysAddress"];
                            DeviceTable.Rows.Find(Dev.IPAddress)["SubNet Mask"] = Dev.DS.Tables["ipAddrEntry"].Rows.Find(Dev.IPAddress)["ipAdEntNetMask"];
                        }
                    }
                    catch
                    {
                    }





                }
                private void HOSTStoSQL()
                {
                    String CreateDeviceTable = "";
                    String InsertDeviceTable = "";
                    String CreateTable = "";
                    String InsertTable = "";
                    CreateDeviceTable = "CREATE TABLE Device ([PrimaryKey] [bigint] NOT NULL ,";//
                    CreateDeviceTable += "[ID] varchar (500), ";
                    CreateDeviceTable += "[SubNet_Mask] varchar (500), ";
                    CreateDeviceTable += "[MAC_Address] varchar (500), ";
                    CreateDeviceTable += "[Scan_Protocols] varchar (500), ";
                    CreateDeviceTable += "[Name] varchar (500), ";
                    CreateDeviceTable += "[Type] varchar (500), ";
                    CreateDeviceTable += "[Object_ID] varchar (500), ";
                    CreateDeviceTable += "[Description] varchar (500), ";
                    CreateDeviceTable += "[Contact] varchar (500), ";
                    CreateDeviceTable += "[Location] varchar (500), ";
                    CreateDeviceTable += "[Interface_Count] varchar (500))";


                    SqlConnection Conn = new SqlConnection("Data Source=192.168.1.10,1433;Network Library=DBMSSOCN;Initial Catalog=Network;User ID=sa;Password=foobar;");
                    SqlCommand cmd = new SqlCommand();
                    Conn.Open();
                    cmd.Connection = Conn;
                    cmd.CommandText = CreateDeviceTable;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                    }

                    Device Dev = null;
                    for (int i = 0; i < HOSTS.Values.Count; i++)
                    {
                        Dev = (Device)HOSTS.Values[i];
                        String sysName = "";
                        String sysDescr = "";
                        String sysContact = "";
                        String sysLocation = "";
                        String sysObjectID = "";
                        String ifNumber = "";
                        String ifPhysAddress = "";
                        String ipAdEntNetMask = "";
                        String ifIndex = "";
                        String Scan_Protocols = "";
                        if (Dev.ICMP) { Scan_Protocols = "Icmp/"; }
                        if (Dev.NETB) { Scan_Protocols += "NetBios/"; }
                        if (Dev.SNMP) { Scan_Protocols += "Snmp/"; }
                        if (Dev.AGNT) { Scan_Protocols += "Agent/"; }
                        Object Val = null;
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("sysName", "system") + ".0", out Val)) { sysName = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("sysDescr", "system") + ".0", out Val)) { sysDescr = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("sysContact", "system") + ".0", out Val)) { sysContact = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("sysLocation", "system") + ".0", out Val)) { sysLocation = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("sysObjectID", "system") + ".0", out Val)) { sysObjectID = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("ifNumber", "interfaces") + ".0", out Val)) { ifNumber = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("ipAdEntIfIndex", "ipAddrEntry") + "." + Dev.IPAddress.ToString(), out Val)) { ifIndex = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("ipAdEntNetMask", "ipAddrEntry") + "." + Dev.IPAddress.ToString(), out Val)) { ipAdEntNetMask = Val.ToString(); }
                        if (Dev.OIDS.TryGetValue(MIBS.MIBtoOID("ifPhysAddress", "ifEntry") + "." + ifIndex, out Val)) { ifPhysAddress = Val.ToString(); }

                        InsertDeviceTable = "INSERT INTO Device ([PrimaryKey],[ID],[SubNet_Mask],[MAC_Address],[Scan_Protocols],[Name],[Type],[Object_ID],[Description],[Contact],[Location],[Interface_Count]) VALUES (";
                        InsertDeviceTable += IP_INDEX(Dev.IPAddress) + ", '";
                        InsertDeviceTable += Dev.IPAddress.ToString() + "', '";
                        InsertDeviceTable += ipAdEntNetMask+ "', '";
                        InsertDeviceTable += ifPhysAddress+ "', '";
                        InsertDeviceTable += Scan_Protocols+ "', '";
                        InsertDeviceTable += sysName + "', '";
                        InsertDeviceTable += Dev.TYPE.Trim()+ "', '";
                        InsertDeviceTable += sysObjectID.Trim()+ "', '";
                        InsertDeviceTable += sysDescr.Trim()+ "', '";
                        InsertDeviceTable += sysContact.Trim()+ "', '";
                        InsertDeviceTable += sysLocation.Trim()+ "', '";
                        InsertDeviceTable += ifNumber.Trim() + "');";
                        cmd.CommandText = InsertDeviceTable;
                        cmd.ExecuteNonQuery();


                        for (int dt = 0; dt < Dev.DS.Tables.Count; dt++)
                        {

                            CreateTable = "CREATE TABLE " + Dev.DS.Tables[dt].TableName + " ([PrimaryKey] [bigint] NOT NULL , ";//
                            for (int dc = 0; dc < Dev.DS.Tables[dt].Columns.Count; dc++)
                            {
                                CreateTable += "[" + Dev.DS.Tables[dt].Columns[dc].ColumnName + "] varchar (500)";
                                if (dc < Dev.DS.Tables[dt].Columns.Count - 1)
                                {
                                    CreateTable += ", ";
                                }
                                else
                                {
                                    CreateTable += ");";
                                }

                            }
                            cmd.CommandText = CreateTable;
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch
                            {
                            }

                            for (int dr = 0; dr < Dev.DS.Tables[dt].Rows.Count; dr++)
                            {
                                InsertTable = "INSERT INTO " + Dev.DS.Tables[dt].TableName + " ([PrimaryKey],";
                                for (int dc = 0; dc < Dev.DS.Tables[dt].Columns.Count; dc++)
                                {
                                    InsertTable += "[" + Dev.DS.Tables[dt].Columns[dc].ColumnName + "]";
                                    if (dc < Dev.DS.Tables[dt].Columns.Count - 1)
                                    {
                                        InsertTable += ",";
                                    }
                                    else
                                    {
                                        InsertTable += ") VALUES ( " + IP_INDEX(Dev.IPAddress) + ",'";
                                    }
                                }
                                for (int dc = 0; dc < Dev.DS.Tables[dt].Columns.Count; dc++)
                                {
                                    InsertTable += Dev.DS.Tables[dt].Rows[dr][dc].ToString();
                                    if (dc < Dev.DS.Tables[dt].Columns.Count - 1)
                                    {
                                        InsertTable += "', '";
                                    }
                                    else
                                    {
                                        InsertTable += "')";
                                    }
                                }
                                cmd.CommandText = InsertTable;
                                try
                                {
                                    cmd.ExecuteNonQuery();
                                }
                                catch
                                {
                                }

                            }


                        }




                        
                    
                    }
                        













                    Conn.Close();
                }
                #endregion
            }
        }
    }
}
