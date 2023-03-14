~~~bash
dotnet aspnet-codegenerator razorpage     -m CheckersGame     -dc AppDbContext     -udl     -outDir Pages/CheckersGames     --referenceScriptLibraries    -f
dotnet aspnet-codegenerator razorpage     -m CheckersOption     -dc AppDbContext     -udl     -outDir Pages/CheckersOptions     --referenceScriptLibraries    -f
dotnet aspnet-codegenerator razorpage     -m CheckersGameState     -dc AppDbContext     -udl     -outDir Pages/CheckersGameStates     --referenceScriptLibraries    -f
~~~