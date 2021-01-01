param(
    [Parameter(Position=0,Mandatory=$False)]        
    [String]$Username = "LOCCIONI\Administrator",# $env:USERDOMAIN + "\" + $env:USERNAME,

    [Parameter(Position=1,Mandatory=$True)]        
    [String]$Password,

    [Parameter(Position=2,Mandatory=$False)]        
    [String]$Server = $env:COMPUTERNAME,

    [Parameter(Position=3,Mandatory=$False)]        
    [String]$NetworkShareName = "D$",

    [Parameter(Position=4,Mandatory=$False)]        
    [String]$DestinationPathInShare = "\Shared\deployment_test", 

    [Parameter(Position=5,Mandatory=$False)]        
    [String]$DeployableServicePath = "C:\Projects\DigitalInvoice\src\DigitalInvoice.Host\bin\Debug",

    [Parameter(Position=6,Mandatory=$False)]        
    [String]$ServiceExecutableFullPathOnServer = "D:\Shared\deployment_test\DigitalInvoice.Host.exe",

	[Parameter(Position=7,Mandatory=$False)]        
    [String]$ServiceName = "DigitalInvoice_TEST"

)

Class Deployer {
    $credentials
    $service
	$driveToMap = "Y:"
    
	[String]$Username
	[String]$Password
    [String]$Server
    [String]$NetworkShare
	[String]$DestinationPathInShare
    [String]$DestinationPath
    [String]$DeployableServicePath
    [String]$DeployableAppPath
	[String]$ServiceExecutableFullPathOnServer
	[String]$ServiceName

    Deployer( 
        [String]$Username, 
        [String]$Password, 
        [String]$Server,                            # solo hostname Es.: GL-WEBAPP02
        [String]$NetworkShareName,                  # Solo il nome della share di rete
        [String]$DestinationPathInShare,            # Il path all'interno della share di rete'
        [String]$DeployableServicePath,             # Il path locale della directory che contiene il servizio. Tipicamente .\bin\<Configuration>
        [String]$DeployableAppPath,                 # Il path locale della applicazione web. Tipicamente .\app
        [String]$ServiceExecutableFullPathOnServer, # Percorso completo del servizio sul server
		[String]$ServiceName                        # Nome del servizio che andrà installato.
    ) {
		$this.Username = $Username
		$this.Password = $Password
        $this.Server = $Server
        $this.NetworkShare = "\\" + $Server + "\" + $NetworkShareName
		$this.DestinationPathInShare = $DestinationPathInShare
        $this.DestinationPath = $this.NetworkShare + "\" + $DestinationPathInShare
        $this.DeployableServicePath = $DeployableServicePath
        $this.DeployableAppPath = $DeployableAppPath
		$this.ServiceExecutableFullPathOnServer = $ServiceExecutableFullPathOnServer
		$this.ServiceName = $ServiceName

		$this.BuildCredentials($Username, $Password)
    }

	BuildCredentials($Username, $Password) {
        $securePassword = ConvertTo-SecureString -AsPlainText $Password -Force
        $this.credentials = New-Object System.Management.Automation.PSCredential -ArgumentList $Username,$securePassword
		# $this.credentials = Get-Credential $Username
	}

    [Boolean]VerifyPreConditions() {
		Write-Host "Verifying pre-conditions"

		Write-Host "*" $this.Server "reachability..."
        If ((Test-Connection -ComputerName $this.Server -Quiet) -ne $True) {
            Write-Host "Destination Server " + $this.Server + " is unreacheable!"
            return $False
        }
        <#
		Write-Host "*" $this.NetworkShare "existence..."
        If ((Test-Path $this.NetworkShare ) -ne $True) {
            Write-Host "Destination Share " + $this.NetworkShare + " does not exist!"
            return $False
        }
        
		Write-Host "*" $this.DestinationPath "existence..."
        If ((Test-Path $this.DestinationPath -Credential $this.credentials) -ne $True) {
            Write-Host "Destination Path " + $this.DestinationPath + " does not exist, will try to create destination dir"
            If($this.TryCreateDestPath() -ne $true) {
                Write-Host "Unable to create destination directory " + $this.DestinationPath
                return $False
            }
        }
        #>
        return $True
    }

    [Boolean]TryCreateDestPath() {
        try {
            New-Item -ItemType Directory -Path $this.DestinationPath -Credential $this.credentials -Force -ErrorAction Stop
            return $true
        } 
        catch {
            return $false
        }
    }

    [Boolean]TryStopService() {
        Write-Host "Stopping service" $this.ServiceName "on" $this.Server
        If ($this.service -ne $null) {
            try {
                $this.service.StopService() # | Stop-Service -Force -ErrorAction Stop
                Start-Sleep -s 10
                Write-Host "Service" $this.ServiceName "on" $this.Server "stopped!"
                return $True
            } catch {
                Write-Host "Errors stopping service" $this.ServiceName "on" $this.Server 
                return $False
            }
        }
        return $false
    }

    [Boolean]TryStartService() {
        Write-Host "Starting service" $this.ServiceName "on" $this.Server
        If ($this.service -ne $null) {
            try {
                $this.service.StartService() # | Sart-Service -Force -ErrorAction Stop
                Start-Sleep -s 10
                Write-Host "Service" $this.ServiceName "on" $this.Server "started!"
                return $True
            } catch {
                Write-Host "Errors starting service" $this.ServiceName "on" $this.Server 
                return $False
            }
        }
        return $false
    }
    
    TryUninstallService() {
		Write-Host "Trying to uninstall service " $this.ServiceName
        If ($this.service -ne $null) {
            $this.service.Delete()
            Write-Host "Waiting for service uninstall to complete..."
            Start-Sleep -s 10
            Write-Host "... done"
        }
    }

    TryInstallService() {
        
        $this.CacheCredentials()       
        
        # $ServiceInstallCommand = $this.ServiceExecutableFullPathOnServer + " install --localsystem "
        $ServiceInstallCommand = $PSScriptRoot + "\psexec \\" + $this.Server + " -accepteula -nobanner -s -u " + $this.Username + " -p " + $this.Password + " " + $this.ServiceExecutableFullPathOnServer + " install -servicename " + $this.ServiceName + " --localsystem"
		Write-Host "Trying to install service " $this.ServiceName "using command" $ServiceInstallCommand
        $scriptBlock = ([ScriptBlock]::create($ServiceInstallCommand))
        try {
            #Invoke-Command -Credential $this.credentials -ComputerName $this.Server -ScriptBlock $scriptBlock
            &$scriptBlock

            Write-Host "Waiting for service install to complete..."
            Start-Sleep -s 5
            Write-Host "... done"
        } catch {
            Write-Host "Errors installing service"
        } finally {
            $this.UncacheCredentials()
        }
    }

    CacheCredentials() {
        # Cache credentials before executing the command
        $command = "cmdkey /add:" + $this.Server + " /user:" + $this.Username + " /pass:" + $this.Password
        $sb=[scriptblock]::create($command);
        &$sb
    }

    UncacheCredentials() {
        # Cache credentials before executing the command
        $command = "cmdkey /delete:" + $this.Server
        $sb=[scriptblock]::create($command);
        &$sb
    }

	TryUnmapDrive() {
		$net = New-Object -ComObject WScript.Network
		try {
			$net.RemoveNetworkDrive($this.driveToMap)
		} catch {
		}	
	}

	TryMapDrive() {
		$net = New-Object -ComObject WScript.Network
		try {
			$net.MapNetworkDrive($this.driveToMap, $this.NetworkShare, $false, $this.Username, $this.Password)
		} catch {
			Write-Host "Error Mapping Drive"	
		}
	
	}

    TryCopyFiles() {
        Write-Host "Copying service from" $this.DeployableServicePath "to" $this.DestinationPath

		$this.TryMapDrive()

		$mappedDestination = $this.driveToMap+"\"+$this.DestinationPathInShare
        $filesToCopy = $this.DeployableServicePath
        # $result = Copy-Item $filesToCopy $mappedDestination -Force -Recurse -Exclude "Logs\*.*" -ErrorAction Continue
		&ROBOCOPY $filesToCopy $mappedDestination /MIR /TEE /FFT /E /S /LOG:robocopy_log.txt /XD Logs /XD App_Data /W:1 /R:20
		$robocopyResult = $LASTEXITCODE
		Get-Content -Tail 13 .\robocopy_log.txt | Write-Host
		$this.TryUnmapDrive()
		# See https://ss64.com/nt/robocopy-exit.html
		if ($robocopyResult -eq 16 ) { 
			Write-Host "*** FATAL ERROR ***. Please check Robocopy log file for details"
			exit -1
		}
		if ($robocopyResult -eq 15 ) { 
			Write-Host "OKCOPY + FAIL + MISMATCHES + XTRA. Please check Robocopy log file for details"
			exit -1 
		}
		if ($robocopyResult -eq 14 ) { 
			Write-Host "FAIL + MISMATCHES + XTRA. Please check Robocopy log file for details"
			exit -1          
		}
		if ($robocopyResult -eq 13 ) { 
			Write-Host "OKCOPY + FAIL + MISMATCHES. Please check Robocopy log file for details"
			exit -1        
		}
		if ($robocopyResult -eq 12 ) { 
			Write-Host "FAIL + MISMATCHES. Please check Robocopy log file for details"
			exit -1                 
		}
		if ($robocopyResult -eq 11 ) { 
			Write-Host "OKCOPY + FAIL + XTRA. Please check Robocopy log file for details"
			exit -1              
		}
		if ($robocopyResult -eq 10 ) { 
			Write-Host "FAIL + XTRA. Please check Robocopy log file for details"
			exit -1                       
		}
		if ($robocopyResult -eq 9  ) { 
			Write-Host "OKCOPY + FAIL. Please check Robocopy log file for details"
			exit -1                     
		}
		if ($robocopyResult -eq 8  ) { 
			Write-Host "FAIL. Please check Robocopy log file for details"
			exit -1                              
		}
		if ($robocopyResult -eq 7  ) { 
			Write-Host "OKCOPY + MISMATCHES + XTRA. Please check Robocopy log file for details"        
			exit -1
		}
		if ($robocopyResult -eq 6  ) { 
			Write-Host "MISMATCHES + XTRA. Please check Robocopy log file for details"                 
			exit -1
		}
		if ($robocopyResult -eq 5  ) { 
			Write-Host "OKCOPY + MISMATCHES. Please check Robocopy log file for details"               
			exit -1
		}
		if ($robocopyResult -eq 4  ) { 
			Write-Host "MISMATCHES. Please check Robocopy log file for details"
			exit -1                        
		}
		if ($robocopyResult -eq 3  ) { 
			Write-Host "OKCOPY + XTRA. Everything was fine!"
		}
		if ($robocopyResult -eq 2  ) { 
			Write-Host "XTRA. Everything was fine!"
		}
		if ($robocopyResult -eq 1  ) { 
			Write-Host "OKCOPY. Everything was fine!"
		}
		if ($robocopyResult -eq 0  ) { 
			Write-Host "No Change"       
		} 
    }

    [Boolean]TryObtainService() {
        Write-Host "Obtaining Service Instance for" $this.ServiceName "on" $this.Server "..."
        $filter = "Name='" + $this.ServiceName + "'"
        try {
            $this.service = Get-WmiObject Win32_Service -ComputerName $this.Server -Credential $this.credentials -Filter $filter -ErrorAction Stop
            If (!$this.service) {
                Write-Host "Service not installed!"
                return $False
            }
            Write-Host "Service instance obtained!"
            return $True
        }
        catch {
            Write-Host "... Something went wrong while retrieving service instance"
            Write-Host $_.Exception.Message
            exit
        }
    }

    Deploy() {
        If(!$this.VerifyPreConditions()) {
            Write-Host "Preconditions not met. Exiting!"
            exit
        }

        $this.TryObtainService()
        
        If($this.service) {
            $this.TryStopService()
            $this.TryUninstallService()
        }
        
        $this.TryCopyFiles();
        
		$maxRetries = 3
		$currentTry = 1
		$installSucceded = $False
		while ($currentTry -le $maxRetries -and !$installSucceded){
			Write-Host "Service install try" $currentTry "of" $maxRetries "..."
			$this.TryInstallService()
			Start-Sleep -Seconds 5
			If ($this.TryObtainService() -eq $True) {
		        $this.TryStartService()
				$installSucceded = $True
				break;
			}
			$currentTry = $currentTry + 1
		}
        
    }
}

[Deployer]$deploy = [Deployer]::new($Username, $Password, $Server, $NetworkShareName, $DestinationPathInShare, $DeployableServicePath, $DeployableAppPath, $ServiceExecutableFullPathOnServer, $ServiceName)
$deploy.Deploy()
