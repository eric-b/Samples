## Dependencies

### Environment variables

- `TF_VAR_azurite_hostpath` (example of value: `/run/desktop/mnt/host/c/users/username/azurite-docker`)

### User files in `%APPDATA%\OpenTofu\dev-dependencies`

- **state.config** containing (notice the directory of file path defined is the same where this file is placed):

```
path = "C:/Users/<username>/AppData/Roaming/OpenTofu/dev-dependencies/state/terraform.tfstate"
```

- **terraform.tfvars** containing:

```
sqlserver_sa_password = "<password>"
```

These two files located here are serving a single purpose: put static secrets required by OpenTofu in a distant location from other OpenTofu workspace files.

## OpenTofu commands

``` powershell
# Init command (to do once)
opentofu init -backend-config $env:APPDATA\OpenTofu\dev-dependencies\state.config

# Plan/Apply command
opentofu plan -var-file $env:APPDATA\OpenTofu\dev-dependencies\terraform.tfvars
opentofu apply -var-file $env:APPDATA\OpenTofu\dev-dependencies\terraform.tfvars

# Destroy command
opentofu destroy -var-file $env:APPDATA\OpenTofu\dev-dependencies\terraform.tfvars
```