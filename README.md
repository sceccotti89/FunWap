# FunWap

This is a toy project used to implement a C# parser for the FunW@p language.
You can find the grammar structure in the Grammar.txt file.

The parser accepts only LL1 grammars, compiling the source code into a C# equivalent code,
including lambda functions. The test the program just use a class like this:

```bash
Parser p = new Parser( new Tokenizer( file ) );
Compiler c = new Compiler( file );
c.compile( p.parse() );
```

You can find an example of that under the test package.

The program can be launched with the following commands within the VS compiler:

```bash
csc Launcher.cs
Launcher "file_path"
```
