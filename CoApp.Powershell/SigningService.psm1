function Set-SignatureViaService
{
    param(
    [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline= $true)]
    [ValidateNotNullOrEmpty()]
    [string[]] $FilePath,
    
    [Parameter(Position = 1)]
    [string] $OutputPath,

    [Parameter()]
    [switch] $StrongName,
    
    [Parameter()]
    [PSCredential]$Credential,

    [Parameter()]
    [string] $ServiceUrl
    )

    trap {
  Write-Error -Exception $_.Exception -Message @"
Error in script $($_.InvocationInfo.ScriptName) :
$($_.Exception) $($_.InvocationInfo.PositionMessage)
"@
 break;
    }



    #get azure-location
    $container = Get-UploadLocation -Remote -ServiceUrl $ServiceUrl -Credential $Credential
    
    $azureCred = Get-AzureCredentials -Remote -ServiceUrl $ServiceUrl -Credential $Credential -ContainerName $container[0]

    new-psdrive -name temp -psprovider azure -root $container[1] -credential $azureCred

	Copy-ItemEx $FilePath -Destination temp:


    pushd temp:
    
    # upload file
    
	
	$files = ls .
	
	
	foreach ($i in $files)
	{
	
		Set-CodeSignature $i -Remote -ServiceUrl $ServiceUrl -Credential $Credential
	}
    
     
    
    if (!$OutputPath)
    {
        $OutputPath = $FilePath
    }

	popd
	Write-Host $OutputPath
    #download file
    Copy-ItemEx temp: -Destination $OutputPath -Force -Verbose
    
    
	Remove-PSDrive temp
}

