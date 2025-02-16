These files generally do not need any update during lifetime of the project.

There are two OpenTofu (fork of Terraform) projects:

- dev-dependencies: development dependencies (Azurite, Azure Service Bus Emulator, Prometheus)
- demo: our application.

Usually, an environment is represented by a single OpenTofu project with multiple modules.
In our specific use case though, it's easier to manage two OpenTofu projects, so we can destroy resources of our app without destroying development dependencies we may need for other applications. Development dependencies may also be useful for local debugging.


Download OpenTofu here: https://opentofu.org/docs/intro/install/standalone/