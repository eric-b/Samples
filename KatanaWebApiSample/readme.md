## Katana WebApi Sample

As the name implies, this is a quick start example to show the use of [WebApi][2] in an [OWIN][1] self hosted server.

Unlike many examples, this one contains two projects :

* **WebApiLib** : application domain (exposed through [WebApi][2] controllers)
* **Host**: [OWIN][1] Self Host (console app) - does not contains any no application logic!

In the OWIN terminology : 

* the first one is a *Web Application*,
* the latter is the *Host* and contains the *Server* ([Katana][3]).
* WebApi is a *Web Framework*
* and, for the sake of example, we use [CacheCow][4] as a *Middleware*.

Blog post (in french) : [http://blog.eric-bml.net/2014/06/self-host-web-api-2-avec-owin-katana.html](http://blog.eric-bml.net/2014/06/self-host-web-api-2-avec-owin-katana.html)

[1]: http://owin.org/#spec "OWIN spec"
[2]: http://www.asp.net/web-api "ASP.NET Web Api"
[3]: http://katanaproject.codeplex.com/ "Katana on CodePlex"
[4]: https://github.com/aliostad/CacheCow/wiki "CacheCow on GitHub"

