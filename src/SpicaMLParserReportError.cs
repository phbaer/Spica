using System;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Castor;

namespace Spica
{
    public class SpicaMLParserReportError : SpicaMLParser
    {
        public SpicaMLParserReportError(ITokenStream input) : base(input)
        {
        }

        public override void ReportError(RecognitionException re)
        {
            string hdr = GetErrorHeader(re);
            string msg = GetErrorMessage(re, this.TokenNames);
            throw new Castor.CException("{0} {1}", hdr, msg);
        }
    }
}

