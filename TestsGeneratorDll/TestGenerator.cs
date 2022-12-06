using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace TestsGeneratorDll
{
    public class TestGenerator
    {
        public List<TestClass> Generate(string sourceCode)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
            List<TestClass> result = new List<TestClass>();
            var classMethods = AnalyzeRoot(root);
            foreach (var member in classMethods)
            {
                string sourceClassName = member.Key.Identifier.Text;
                var testClass = SyntaxFactory.CompilationUnit();
                string namespaceName = NamespaceName(member.Key);
                testClass = testClass.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName)));
                var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{namespaceName}.Tests"));
                testClass = testClass.AddUsings(root.Usings.ToArray());
                testClass = testClass.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("NUnit.Framework")));
                testClass = testClass.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Moq")));

                ClassDeclarationSyntax classDeclaration = SyntaxFactory.ClassDeclaration($"{sourceClassName}Tests");
                classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                //adding setup method
                CreateSetUpMethod(member.Key, ref classDeclaration);

                classDeclaration = classDeclaration.AddMembers(GenerateTestMethods(member.Value, sourceClassName).ToArray());
                @namespace = @namespace.AddMembers(classDeclaration);
                testClass = testClass.AddMembers(@namespace);

                TestClass @class = new TestClass(sourceClassName, testClass.NormalizeWhitespace().ToFullString());

                result.Add(@class);
            }
            return result;
        }

        public string NamespaceName(ClassDeclarationSyntax classDeclaration)
        {
            string namespaceName = "";

            var @namespace = classDeclaration.Parent as NamespaceDeclarationSyntax;
            if(@namespace != null)
            {
                namespaceName = @namespace.Name.ToString();
            }

            return namespaceName;
        }
        public List<MethodDeclarationSyntax> GenerateTestMethods(List<MethodDeclarationSyntax> sourceMethods, string sourceClassName)
        {
            List<MethodDeclarationSyntax> methods = new List<MethodDeclarationSyntax>();
            List<string> methodNames = new List<string>();


            string initChar = sourceClassName[0].ToString().ToLower();
            string className = sourceClassName.Remove(0, 1);
            className = $"_{initChar}{className}UnderTest";

            foreach (MethodDeclarationSyntax method in sourceMethods)
            {
                string testMethodName = method.Identifier.ToString();
                while(methodNames.Contains(testMethodName)) 
                {
                    testMethodName += "1";
                }
                testMethodName += "Test";


                string identifierInvoceName = method.Modifiers.Any(SyntaxKind.StaticKeyword) ? sourceClassName : className;

                var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), testMethodName);
                methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                methodDeclaration = methodDeclaration.AddAttributeLists(
                    SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Test")))));

                var returnType = method.ReturnType;
                var parameters = method.ParameterList.Parameters;
                var arguments = new SeparatedSyntaxList<ArgumentSyntax>();
                foreach (var parameter in parameters)
                {
                    TypeSyntax? type = parameter.Type;
                    if (type != null)
                    {
                        string varName = parameter.Identifier.ToString();
                        
                        initChar = varName[0].ToString().ToLower();
                        varName = varName.Remove(0, 1);
                        varName = $"_{initChar}{varName}";
                        
                        var typeString = type.ToString();
                        var argument = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(varName));

                        if (typeString[0].Equals('I') && typeString[1].ToString().ToUpper().Equals(typeString[1].ToString().ToUpper()))
                        {
                            var mockName = SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("Mock"))
                                .WithTypeArgumentList(
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        type)));

                            methodDeclaration = methodDeclaration.AddBodyStatements(
                                SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(mockName)
                                .WithVariables(
                                    SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.VariableDeclarator(varName)
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(SyntaxFactory.ObjectCreationExpression(mockName)
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList())))))));



                            argument = SyntaxFactory.Argument(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                               SyntaxFactory.IdentifierName(varName), SyntaxFactory.IdentifierName("Object")));
                        }
                        else
                        {
                            var expr = GetDefault(type.ToString());
                            methodDeclaration = methodDeclaration.AddBodyStatements(
                               SyntaxFactory.LocalDeclarationStatement(
                               SyntaxFactory.VariableDeclaration(type)
                               .WithVariables(
                                   SyntaxFactory.SingletonSeparatedList(
                                   SyntaxFactory.VariableDeclarator(varName)
                               .WithInitializer(
                                   SyntaxFactory.EqualsValueClause(expr))))));
                        }
                        arguments = arguments.Add(argument);
                    }
                }
                if (!returnType.ToString().Equals("void"))
                {
                    methodDeclaration = methodDeclaration.AddBodyStatements(
                        SyntaxFactory.LocalDeclarationStatement(
                            SyntaxFactory.VariableDeclaration(returnType)
                            .WithVariables(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.VariableDeclarator(
                                        SyntaxFactory.Identifier("actual"))
                                    .WithInitializer(
                                        SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.InvocationExpression(
                                               SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                               SyntaxFactory.IdentifierName(identifierInvoceName),
                                               SyntaxFactory.IdentifierName(method.Identifier.Text)))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(arguments))
                                                ))))));



                    var expr = GetDefault(returnType.ToString());
                    methodDeclaration = methodDeclaration.AddBodyStatements(
                               SyntaxFactory.LocalDeclarationStatement(
                               SyntaxFactory.VariableDeclaration(returnType)
                               .WithVariables(
                                   SyntaxFactory.SingletonSeparatedList(
                                   SyntaxFactory.VariableDeclarator(
                                           SyntaxFactory.Identifier("expected"))
                               .WithInitializer(
                                   SyntaxFactory.EqualsValueClause(
                                       expr))))));


                    var IsEqualArg = SyntaxFactory.Argument(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("Is"),
                                        SyntaxFactory.IdentifierName("EqualTo")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.IdentifierName(
                                                        "expected"))))));

                    var arglist = SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("actual")));
                    arglist = arglist.Add(IsEqualArg);

                    methodDeclaration = methodDeclaration.AddBodyStatements(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                               SyntaxKind.SimpleMemberAccessExpression,
                               SyntaxFactory.IdentifierName("Assert"), SyntaxFactory.IdentifierName("That")))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(arglist))));

                }
                else
                {
                    methodDeclaration = methodDeclaration.AddBodyStatements(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(identifierInvoceName), SyntaxFactory.IdentifierName(method.Identifier.Text)))
                            .WithArgumentList(SyntaxFactory.ArgumentList(arguments))));
                }
                methodDeclaration = methodDeclaration.AddBodyStatements(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                               SyntaxKind.SimpleMemberAccessExpression, 
                               SyntaxFactory.IdentifierName("Assert"), SyntaxFactory.IdentifierName("Fail")),
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("autogenerated")
                                            )))))));
                
                methods.Add(methodDeclaration);
            }
            return methods;
        }
        public void CreateSetUpMethod(ClassDeclarationSyntax sourceClass, ref ClassDeclarationSyntax testClassDeclaration)
        {
            if(sourceClass.Modifiers.Any(SyntaxKind.StaticKeyword)) { return; }

            string sourceClassName = sourceClass.Identifier.Text;
            string initChar = sourceClassName[0].ToString().ToLower();
            string className = sourceClassName.Remove(0, 1);
            className = $"_{initChar}{className}UnderTest";


            var fieldClass = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName(sourceClassName))
                .AddVariables(SyntaxFactory.VariableDeclarator(className)))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

            testClassDeclaration = testClassDeclaration.AddMembers(fieldClass);

            MethodDeclarationSyntax methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "SetUp");
            methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            methodDeclaration = methodDeclaration.AddAttributeLists(
                SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("SetUp")))));

            var constructorDeclarations = (from constructorDeclaration in sourceClass.DescendantNodes().OfType<ConstructorDeclarationSyntax>()
                                          where constructorDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
                                          select constructorDeclaration).FirstOrDefault();

            ObjectCreationExpressionSyntax creationExpressionSyntax = SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.IdentifierName(
                        sourceClassName))
                       .WithArgumentList(
                           SyntaxFactory.ArgumentList());

            if (constructorDeclarations != null)
            {
                var testConstructor = SyntaxFactory.ConstructorDeclaration(sourceClassName);
                var parameters = constructorDeclarations.ParameterList.Parameters;

                var arguments = new SeparatedSyntaxList<ArgumentSyntax>();

                foreach(var parameter in parameters)
                {

                    TypeSyntax? type = parameter.Type;
                    if(type != null)
                    {
                        string fieldName = parameter.Identifier.ToString();
                        initChar = fieldName[0].ToString().ToLower();
                        fieldName = fieldName.Remove(0, 1);
                        fieldName = $"_{initChar}{fieldName}";
                        var typeString = type.ToString();

                        ArgumentSyntax argument = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(fieldName));

                        if (typeString[0].Equals('I') && typeString[1].ToString().ToUpper().Equals(typeString[1].ToString().ToUpper()))
                        {
                            var mockName = SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier("Mock"))
                                .WithTypeArgumentList(
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        type)));

                            argument = SyntaxFactory.Argument(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                               SyntaxFactory.IdentifierName(fieldName), SyntaxFactory.IdentifierName("Object")));

                            testClassDeclaration = testClassDeclaration.AddMembers(
                                SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(mockName)
                                        .AddVariables(SyntaxFactory.VariableDeclarator(fieldName)))
                                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

                            methodDeclaration = methodDeclaration.AddBodyStatements(
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(fieldName),
                                    SyntaxFactory.ObjectCreationExpression(mockName)
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList()))));
                        }
                        else
                        {
                            testClassDeclaration = testClassDeclaration.AddMembers(
                                SyntaxFactory.FieldDeclaration(
                                    SyntaxFactory.VariableDeclaration(
                                        type)
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator(fieldName)))
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

                            var expr = GetDefault(type.ToString());

                            methodDeclaration = methodDeclaration.AddBodyStatements(
                                SyntaxFactory.ExpressionStatement(
                                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(fieldName), expr)));

                        }
                        arguments = arguments.Add(argument);

                    }
                }

                creationExpressionSyntax = SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.IdentifierName(
                        sourceClassName));

                if(arguments.Count > 0)
                {
                    creationExpressionSyntax = creationExpressionSyntax.WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            arguments));
                }
            }

            ExpressionStatementSyntax initClass = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(className),
                creationExpressionSyntax));
            methodDeclaration = methodDeclaration.AddBodyStatements(initClass);
            testClassDeclaration = testClassDeclaration.AddMembers(methodDeclaration);
        }
        public Dictionary<ClassDeclarationSyntax, List<MethodDeclarationSyntax>> AnalyzeRoot(CompilationUnitSyntax root)
        {
            var usings = root.Usings;
            var members = root.Members;

            Dictionary<ClassDeclarationSyntax, List<MethodDeclarationSyntax>> classMethods = new();

            var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToList();
            var classes = from classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                           where classDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
                           select classDeclaration;

            foreach (var classDeclaration in classes)
            {
                var methods = from methodDeclaration in classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
                             where methodDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
                             select methodDeclaration;

                classMethods.Add(classDeclaration, methods.ToList());
            }
            return classMethods;
        }

        public ExpressionSyntax GetDefault(string t)
        {

            ExpressionSyntax expr = SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword)); 
            if (t.ToLower().EndsWith("?"))
            {
                expr = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            } else if (t.ToLower().Equals("int") || t.ToLower().Equals("int32") || t.ToLower().Equals("long") || t.ToLower().Equals("byte"))
            {
                expr = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));
            }
            else if(t.ToLower().Equals("string"))
            {
                expr = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(""));
            }

            return expr;
        }
    }
}