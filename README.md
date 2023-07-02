<img src="Images/ReadMeBanner.png" alt="Monogame.ImGuiNet_Logo" width="100%">
!\[my badge\](https://badgen.net/badge/hello/world/red?icon=twitter)

# Monogame.ImGuiNet
Monogame.ImGuiNet is a wrapper for ImGui.Net specifically designed for use with Monogame.

# Getting Started

The project is a NuGet Package, which could be installed by typing the following command in your Project Command Line Interface (CLI):
`dotnet add package Monogame.ImGuiNet`

Once installed in your project, you can use the library by typing:
`using Monogame.ImGuiNet`

In your game's Initializtion code, create an instance of the ImGuiRenderer class:
```cs
private ImGuiRenderer _imGuiRenderer;

protected override void Initialize()
{
    // ...

    _imGuiRenderer = new ImGuiRenderer(this);
    _imGuiRenderer.RebuildFontAtlas();
    
    // ...
}
```

Next, in your game's **Draw** method, draw the Interface:
```cs
protected override void Draw(GameTime gameTime)
{
    // ...
    
    GuiRenderer.BeginLayout(gameTime);

    // ImGui Code

    GuiRenderer.EndLayout();

    // ...
}
```

And you should be good to go!

It'll be recommended to visit the Wiki page! It will provide general information on the library, Usage examples, as well as every method you could use currently!

# Notable Mentions

[Dovker](https://github.com/dovker) - For the original [Monogame.ImGui](https://github.com/dovker/Monogame.ImGui)
