Cette version est une version de ndbunit allégée pour le CEG.

Les modifications apportées sont les suivantes :

- support des bases Oracle uniquement
- pas de diable/enable des contraintes avant/après les différentes opérations
- suppression des opérations pas utilisées (Delete, InsertIdentity et Update)

Pour faire une nouvelle version :

1. Compiler la solution avec la configuration "Release"
2. Modifier la version des l'assembly NDBUnit.Core (fichier AssemblyInfo.cs)
3. Modifier la version dans les fichiers "nuget" (Packaging/*.nuspec) (ne pas oublier de modifier la version de dépendance)
4. Packager et pousser la version sur le serveur NuGet du CEG avec le batch PackAndPush.cmd

