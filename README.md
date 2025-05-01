## Compilação do projeto lambda

Compile o projeto
`dotnet build src/AutenticacaoFunction/src/AutenticacaoFunction/AutenticacaoFunction.csproj`

Publique o projeto
`dotnet publish src/AutenticacaoFunction/src/AutenticacaoFunction/AutenticacaoFunction.csproj -c Release -o ./publish`

Compacte o conteúdo da pasta publish para um arquivo ZIP
```
zip -r lambda_function.zip ./publish/*
mv lambda_function.zip ./terraform
```