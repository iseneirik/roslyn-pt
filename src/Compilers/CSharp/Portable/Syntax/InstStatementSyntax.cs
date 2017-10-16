using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.CSharp.Syntax
{
    public partial class InstStatementSyntax
    {
        new internal Syntax.InternalSyntax.InstStatementSyntax Green
        {
            get { return (Syntax.InternalSyntax.InstStatementSyntax) base.Green; }
        }
    }
}
