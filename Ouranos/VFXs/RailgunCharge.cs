using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace APurpleApple.Shipyard.Ouranos
{
    public class RailgunCharge : FX
    {
        public double intensity = 1;

        public Random rand = new Random();

        public double lastParticleSpawned = 0;

        public Vec loc = new Vec();
        public override void Update(G g)
        {
            Ship ship = g.state.ship;
            int cannonX = ship.parts.FindIndex((Part p) => p.key == "Ouranos_Cannon");

            loc = FxPositions.Cannon(15 + cannonX, true) + new Vec(- ship.parts.Count * 8, 25);

            for (int i = particles.Count-1; i >= 0; i--)
            {
                particles[i].age += g.dt;
                if (particles[i].age > 1)
                {
                    particles.Remove(particles[i]);
                }
            }

            if (g.time - lastParticleSpawned > .15)
            {
                lastParticleSpawned = g.time;
                particles.Add(new Particle(new Vec(loc.x+(rand.NextDouble()-.5)*30, loc.y+(rand.NextDouble() - .5) * 30), 1));
            }
        }

        private List<Particle> particles = new List<Particle>();
        private class Particle
        {
            public Vec pos;
            public double size = 0;
            public double age = 0;
            public Particle(Vec loc, float size)
            {
                pos = loc;
                size = 1;
            }
        }

        public override void Render(G g, Vec v)
        {
            double scale = Math.Min(intensity / 20, 1) + (Math.Sin(g.time * 14) + 2) * 0.1;
            Vec orbLoc = loc + new Vec(-8 * scale, -8 * scale);

            foreach (Particle p in particles)
            {
                double pScale = p.age * .2;
                Vec pLoc = Vec.Lerp(p.pos, loc, Ease.InSin(p.age)) + new Vec(-8 * pScale, -8 * pScale);
                Draw.Sprite(PMod.sprites[PSpr.FX_RailgunCharge].Sprite, pLoc.x, pLoc.y, scale: new Vec(pScale, pScale));
            }

            Draw.Sprite(PMod.sprites[PSpr.FX_RailgunCharge].Sprite, orbLoc.x+.5, orbLoc.y, scale: new Vec(scale, scale));
        }
    }
}
