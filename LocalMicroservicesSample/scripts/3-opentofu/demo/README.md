## Dependencies

### User files in `%APPDATA%\OpenTofu\demo`

- **state.config** containing (notice the directory of file path defined is the same where this file is placed):

```
path = "C:/Users/<username>/AppData/Roaming/OpenTofu/demo/state/terraform.tfstate"
```

- **terraform.tfvars** containing:

```
sqlserver_demo_container_password = "<password>"
```

These two files located here are serving a single purpose: put static secrets required by OpenTofu in a distant location from other OpenTofu workspace files.

## OpenTofu commands

``` powershell
# Init command (to do once)
OpenTofu init -backend-config $env:APPDATA\OpenTofu\demo\state.config

# Plan/Apply command
OpenTofu plan -var-file $env:APPDATA\OpenTofu\demo\terraform.tfvars
OpenTofu apply -var-file $env:APPDATA\OpenTofu\demo\terraform.tfvars

# Destroy command
OpenTofu destroy -var-file $env:APPDATA\OpenTofu\demo\terraform.tfvars
```