using System;
using Antlr.Utility.Tree;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Antlr.StringTemplate;
using Antlr.StringTemplate.Language;

public class DumpTree {
    public static void Main(string[] args) {
        ICharStream input = new ANTLRFileStream(args[0]);
        SpicaMLLexer lex = new SpicaMLLexer(input);
        CommonTokenStream tokens = new CommonTokenStream(lex);
        SpicaMLParser parser = new SpicaMLParser(tokens);
        SpicaMLParser.model_return r = parser.model();
        ITree t = (ITree)r.Tree;
//        Console.Out.WriteLine(t.ToStringTree());
        DOTTreeGenerator gen = new DOTTreeGenerator();
        StringTemplate st = gen.ToDOT(t);
        Console.Out.WriteLine(st); 
    }
}
