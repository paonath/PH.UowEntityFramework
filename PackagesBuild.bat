echo "Build nuget packages"



dotnet pack "P:\Dev\Gitlab\PH.UowEntityFramework\src\PH.UowEntityFramework\PH.UowEntityFramework.UnitOfWork\PH.UowEntityFramework.UnitOfWork.csproj" -c Release --include-symbols --include-source  -o "P:\Dev\Gitlab\PH.UowEntityFramework\BuildPackages"


dotnet pack "P:\Dev\Gitlab\PH.UowEntityFramework\src\PH.UowEntityFramework\PH.UowEntityFramework.EntityFramework.Abstractions\PH.UowEntityFramework.EntityFramework.Abstractions.csproj" -c Release --include-symbols --include-source -o "P:\Dev\Gitlab\PH.UowEntityFramework\BuildPackages"
dotnet pack "P:\Dev\Gitlab\PH.UowEntityFramework\src\PH.UowEntityFramework\PH.UowEntityFramework.EntityFramework.Abstractions.Identity\PH.UowEntityFramework.EntityFramework.Abstractions.Identity.csproj" -c Release --include-symbols --include-source -o "P:\Dev\Gitlab\PH.UowEntityFramework\BuildPackages"

dotnet pack "P:\Dev\Gitlab\PH.UowEntityFramework\src\PH.UowEntityFramework\PH.UowEntityFramework.EntityFramework\PH.UowEntityFramework.EntityFramework.csproj" -c Release --include-symbols --include-source  -o "P:\Dev\Gitlab\PH.UowEntityFramework\BuildPackages"
dotnet pack "P:\Dev\Gitlab\PH.UowEntityFramework\src\PH.UowEntityFramework\PH.UowEntityFramework.EntityFramework.Identity\PH.UowEntityFramework.EntityFramework.Identity.csproj" -c Release --include-symbols --include-source  -o "P:\Dev\Gitlab\PH.UowEntityFramework\BuildPackages"



echo "done"
