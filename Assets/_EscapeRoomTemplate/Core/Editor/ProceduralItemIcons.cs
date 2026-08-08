using System;
using EscapeRoomRevolt.Systems.Inventory;
using UnityEngine;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>
    /// Draws flat inventory icons for items that have no World Prefab to photograph — notes, keys,
    /// batteries and the like. Shapes are picked from what the item actually is (a readable item is
    /// a note, an id mentioning a key is a key) rather than from the Category field, which in the
    /// shipped content is almost always left at its KeyItem default and would give every item the
    /// same picture.
    /// </summary>
    internal static class ProceduralItemIcons
    {
        private enum Shape { Note, Key, Battery, Fuse, Money, Tape, Flashlight, Crate }

        // Drawn at 2x and box-filtered down, which is what keeps the edges from looking jagged.
        private const int Supersample = 2;

        private static readonly Color Paper = new Color32(223, 207, 158, 255);
        private static readonly Color PaperShade = new Color32(188, 173, 128, 255);
        private static readonly Color Brass = new Color32(176, 148, 88, 255);
        private static readonly Color BrassDark = new Color32(120, 100, 58, 255);
        private static readonly Color Ink = new Color32(58, 52, 38, 255);
        private static readonly Color Steel = new Color32(168, 172, 178, 255);

        public static Texture2D Create(InventoryItemData data, int size)
        {
            Shape shape = ResolveShape(data);
            int hi = size * Supersample;
            var pixels = new Color[hi * hi];

            for (int y = 0; y < hi; y++)
            for (int x = 0; x < hi; x++)
            {
                // Normalised to -1..1 with y pointing up, so the shape code reads geometrically.
                float u = (x + .5f) / hi * 2f - 1f;
                float v = 1f - (y + .5f) / hi * 2f;
                pixels[y * hi + x] = Sample(shape, u, v);
            }

            var high = new Texture2D(hi, hi, TextureFormat.RGBA32, false);
            high.SetPixels(pixels);
            high.Apply();

            Texture2D result = Downsample(high, size);
            UnityEngine.Object.DestroyImmediate(high);
            return result;
        }

        private static Shape ResolveShape(InventoryItemData data)
        {
            string id = ((data.ItemId ?? string.Empty) + " " + data.name + " " + (data.DisplayName ?? string.Empty)).ToLowerInvariant();

            if (data.IsReadable || Contains(id, "note", "nota", "document", "carta")) return Shape.Note;
            if (Contains(id, "key", "clau", "llave")) return Shape.Key;
            if (Contains(id, "batter", "pila", "bateria")) return Shape.Battery;
            if (Contains(id, "fuse", "fusible")) return Shape.Fuse;
            if (Contains(id, "money", "diner", "dinero", "cash")) return Shape.Money;
            if (Contains(id, "tape", "cinta")) return Shape.Tape;
            if (Contains(id, "flashlight", "llanterna", "linterna", "torch")) return Shape.Flashlight;

            return data.Category == InventoryItemCategory.Document ? Shape.Note : Shape.Crate;
        }

        private static bool Contains(string haystack, params string[] needles)
        {
            foreach (string n in needles) if (haystack.Contains(n)) return true;
            return false;
        }

        private static Color Sample(Shape shape, float u, float v)
        {
            switch (shape)
            {
                case Shape.Note: return SampleNote(u, v);
                case Shape.Key: return SampleKey(u, v);
                case Shape.Battery: return SampleBattery(u, v);
                case Shape.Fuse: return SampleFuse(u, v);
                case Shape.Money: return SampleMoney(u, v);
                case Shape.Tape: return SampleTape(u, v);
                case Shape.Flashlight: return SampleFlashlight(u, v);
                default: return SampleCrate(u, v);
            }
        }

        private static Color SampleNote(float u, float v)
        {
            if (!InRect(u, v, .52f, .68f)) return Color.clear;

            // Folded top-right corner.
            if (u > .18f && v > .34f && (u - .18f) + (v - .34f) > .34f)
                return (u - .18f) + (v - .34f) > .40f ? Color.clear : PaperShade;

            for (int i = 0; i < 5; i++)
            {
                float lineY = .40f - i * .21f;
                float halfWidth = i == 4 ? .28f : .38f;
                if (Mathf.Abs(v - lineY) < .022f && Mathf.Abs(u) < halfWidth) return Ink;
            }
            return Paper;
        }

        private static Color SampleKey(float u, float v)
        {
            // Bow: a ring at the top.
            float ringDistance = Mathf.Sqrt(u * u + (v - .46f) * (v - .46f));
            if (ringDistance < .30f) return ringDistance > .14f ? Brass : Color.clear;

            // Shaft running down the middle.
            if (Mathf.Abs(u) < .085f && v < .46f && v > -.72f) return Brass;

            // Two teeth on the right of the shaft.
            if (u > 0f && u < .30f && (Mathf.Abs(v + .34f) < .07f || Mathf.Abs(v + .60f) < .07f)) return Brass;

            return Color.clear;
        }

        private static Color SampleBattery(float u, float v)
        {
            if (Mathf.Abs(u) < .10f && v > .58f && v < .72f) return Steel;      // terminal nub
            if (!InRect(u, v, .34f, .60f)) return Color.clear;

            if (v > .34f) return BrassDark;                                       // top band
            if (Mathf.Abs(u) < .17f && Mathf.Abs(v + .12f) < .04f) return Paper;   // "+" bar
            if (Mathf.Abs(v + .12f) < .17f && Mathf.Abs(u) < .04f) return Paper;   // "+" stem
            return Brass;
        }

        private static Color SampleFuse(float u, float v)
        {
            if (Mathf.Abs(v) > .26f) return Color.clear;
            if (Mathf.Abs(u) > .78f) return Color.clear;
            if (Mathf.Abs(u) > .52f) return Steel;                                 // metal end caps
            if (Mathf.Abs(v) < .035f) return BrassDark;                            // filament
            return new Color(Paper.r, Paper.g, Paper.b, .85f);                      // glass body
        }

        private static Color SampleMoney(float u, float v)
        {
            if (!InRect(u, v, .74f, .44f)) return Color.clear;
            if (!InRect(u, v, .66f, .36f)) return BrassDark;                       // border
            float d = Mathf.Sqrt(u * u + v * v);
            if (d < .20f) return d > .15f ? BrassDark : Paper;                     // centre medallion
            return Brass;
        }

        private static Color SampleTape(float u, float v)
        {
            float d = Mathf.Sqrt(u * u + v * v);
            if (d > .74f) return Color.clear;
            if (d > .60f) return Ink;                                              // outer rim
            if (d > .26f) return BrassDark;                                        // spool body
            if (d > .18f) return Steel;                                            // hub ring
            return Color.clear;                                                    // centre hole
        }

        private static Color SampleFlashlight(float u, float v)
        {
            float rotatedU = (u + v) * .7071f;
            float rotatedV = (v - u) * .7071f;
            if (Mathf.Abs(rotatedV) > .17f) return Color.clear;
            if (rotatedU > .70f || rotatedU < -.78f) return Color.clear;
            if (rotatedU < -.60f) return Paper;                                    // lens
            if (rotatedU < -.42f) return Steel;                                    // head
            return Ink;                                                            // barrel
        }

        private static Color SampleCrate(float u, float v)
        {
            if (!InRect(u, v, .62f, .62f)) return Color.clear;
            if (!InRect(u, v, .54f, .54f)) return BrassDark;                       // edge
            if (Mathf.Abs(u - v) < .09f || Mathf.Abs(u + v) < .09f) return BrassDark; // cross bracing
            return Brass;
        }

        private static bool InRect(float u, float v, float halfWidth, float halfHeight) =>
            Mathf.Abs(u) <= halfWidth && Mathf.Abs(v) <= halfHeight;

        private static Texture2D Downsample(Texture2D source, int size)
        {
            var output = new Color[size * size];
            int factor = Supersample;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float r = 0f, g = 0f, b = 0f, a = 0f;
                for (int sy = 0; sy < factor; sy++)
                for (int sx = 0; sx < factor; sx++)
                {
                    Color c = source.GetPixel(x * factor + sx, y * factor + sy);
                    // Weight colour by alpha so transparent samples don't wash out the edges.
                    r += c.r * c.a; g += c.g * c.a; b += c.b * c.a; a += c.a;
                }
                int samples = factor * factor;
                output[y * size + x] = a <= 0.0001f
                    ? Color.clear
                    : new Color(r / a, g / a, b / a, a / samples);
            }

            var result = new Texture2D(size, size, TextureFormat.RGBA32, false);
            result.SetPixels(output);
            result.Apply();
            return result;
        }
    }
}
