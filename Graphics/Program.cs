using Caliban.Core.Debug;
using Caliban.Core.World;
using Caliban.Graphics;
using Caliban.Graphics.Drawables;
using Caliban.Graphics.Transport;

// Debug mode is toggled by passing "debug" as a launch parameter, mirroring the
// rest of Caliban (e.g. CalibanProgram sets D.debugMode = _args.Contains("debug")).
// Only in debug mode do we spawn a console window; otherwise the overlay runs
// silently as a plain GUI app with no console attached.
D.debugMode = args.Contains("debug");
//if (D.debugMode)
//WinEx.SpawnConsole();

var sandStorm = new SandStorm();

using var app = new App();
app.Spawn("sandstorm", sandStorm);
//app.Spawn("rock1", new Rock(1, Biome.DESERT));
//app.Spawn("rock2", new Rock(2, Biome.DESERT));
//app.Spawn("rock3", new Rock(1, Biome.DESERT));
//app.Spawn("rock4", new Rock(4, Biome.DESERT));
_ = new AppTransportClient(app);
app.Run();