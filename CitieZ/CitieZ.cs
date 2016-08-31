using System;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;

namespace CitieZ
{
    [ApiVersion(1, 23)]
    public class CitieZ : TerrariaPlugin
    {
        public CitieZ(Main game) : base(game)
        {
        }

        public override string Author => "Renerte";
        public override string Name => "CitieZ";
        public override string Description => "Configurable cities system for your TShock server!";
        public override Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        public override void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}