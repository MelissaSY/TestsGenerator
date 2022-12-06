using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorTests
{
    public class AttributeListEqualityComparere : IEqualityComparer<AttributeListSyntax>
    {
        bool IEqualityComparer<AttributeListSyntax>.Equals(AttributeListSyntax? x, AttributeListSyntax? y)
        {
            if(x == null && y == null)
            {
                return false;
            }
            if(x == null && y != null)
            {
                return false;
            }
            if(x!= null && y == null)
            {
                return false;
            }
            SeparatedSyntaxList<AttributeSyntax> xAttr = x.Attributes;
            SeparatedSyntaxList<AttributeSyntax> yAttr = y.Attributes;
            bool equal = (xAttr.Count == yAttr.Count);
            for(int i=0;i<xAttr.Count && equal; i++)
            {
                equal = xAttr[i].ToString().Equals(yAttr[i].ToString());
            }
            return equal;
        }

        int IEqualityComparer<AttributeListSyntax>.GetHashCode(AttributeListSyntax obj)
        {
            throw new NotImplementedException();
        }
    }
}
