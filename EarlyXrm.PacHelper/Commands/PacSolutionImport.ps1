param(
	$ItemPath # path to the *.cdsproj
)

$directory = [System.IO.Path]::GetDirectoryName($ItemPath)

#. .\PacOrgSelect.ps1

cd $directory

pac solution pack -p Unmanaged -z "bin\debug\solution_unmanaged.zip" -f src

pac solution import -p "bin\debug\solution_unmanaged.zip" --async | tee-object -variable importResult

if ($importResult | ? { $_.Conatins("Error") } ) { exit 1 }

pac solution publish --async