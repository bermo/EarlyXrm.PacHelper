param(
	$ItemPath # path to the *.cdsproj
)

$directory = [System.IO.Path]::GetDirectoryName($ItemPath)

#. .\PacOrgSelect.ps1

cd $directory

pac solution sync