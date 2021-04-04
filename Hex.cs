using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HexGrid
{
    /// <summary>
    /// <para> Enumeration describing a direction within a hexagonal grid-space. </para> 
    /// <para> Capital letters indicate a positive value on that axis, lowercase indicates negative values. </para>
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming",
        Justification = "Capitalization of enum values indicates positive/negative values.")]
    public enum Direction
    {
        /// <summary> Obligatory 'Undefined' enumeration value. </summary>
        Undefined = 0,

        /// <summary> Adjacent: X + 1, Z - 1 </summary>
        Xz,

        /// <summary> Adjacent: X + 1, Y - 1 </summary>
        Xy,

        /// <summary> Adjacent: Y + 1, X - 1 </summary>
        Yx,

        /// <summary> Adjacent: Y + 1, Z - 1 </summary>
        Yz,

        /// <summary> Adjacent: Z + 1, X - 1 </summary>
        Zx,

        /// <summary> Adjacent: Z + 1, Y - 1 </summary>
        Zy,

        /// <summary> Diagonal: X + 2, Y - 1, Z - 1 </summary>
        Xyz,

        /// <summary> Diagonal: X - 1, Y + 2, Z - 1 </summary>
        xYz,

        /// <summary> Diagonal: X - 1, Y - 1, Z + 2 </summary>
        xyZ,

        /// <summary> Diagonal: X - 2, Y + 1, Z + 1 </summary>
        xYZ,

        /// <summary> Diagonal: X + 1, Y - 2, Z + 1 </summary>
        XyZ,

        /// <summary> Diagonal: X + 1, Y + 1, Z - 2 </summary>
        XYz
    }

    /// <summary> Enumerations describing a direction of rotational movement. </summary>
    public enum RotationDirection
    {
        /// <summary> Obligatory 'Undefined' enumeration value. </summary>
        Undefined = 0,

        /// <summary> Enumeration describing the 'Clockwise' direction of rotational movement. </summary>
        Clockwise,

        /// <summary> Enumeration describing the 'Counter-Clockwise' direction of rotational movement. </summary>
        CounterClockwise
    }

    /// <summary> A struct which represents a discrete point in 2-D regularly-tiled hexagonal space. </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Struct intended for public use.")]
    public readonly struct Hex : IEquatable<Hex>
    {
        /// <summary> The X-value of this <see cref="Hex"/> struct. </summary>
        public readonly int X;

        /// <summary> The Y-value of this <see cref="Hex"/> struct. </summary>
        public readonly int Y;

        /// <summary> The Z-value of this <see cref="Hex"/> struct.  </summary>
        public readonly int Z;


        /// <summary> Returns a new <see cref="Hex"/> struct with the given [x] and [y] values. The 'Z' value is calculated from [x] and [y]. </summary>
        /// <param name="x"> The 'X' value to assign to this <see cref="Hex"/> struct. </param>
        /// <param name="y"> The 'X' value to assign to this <see cref="Hex"/> struct. </param>
        public Hex(int x, int y)
        {
            X = x;
            Y = y;

            Z = -x - y;
        }


        /// <summary> Returns a new <see cref="Hex"/> value whose X and Y values are equal to 0. </summary>
        public static Hex Zero => new Hex(0, 0);

        /// <summary> Returns a new <see cref="Hex"/> value whose X and Y values are equal to 1. </summary>
        public static Hex One => new Hex(1, 1);

        /// <summary> Returns a <see cref="Tuple"/> representation of this <see cref="Hex"/>. </summary>
        public (int X, int Y) AsTuple => (X, Y);


        /// <summary> Returns a <see cref="Hex"/> vector value based on the given <see cref="Direction"/>. </summary>
        /// <param name="dir"> The <see cref="Direction"/> to acquire a <see cref="Hex"/> vector for. </param>
        public static Hex GetVectorFromDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.Xy: return new Hex(1, -1);
                case Direction.Xz: return new Hex(1, 0);
                case Direction.Yx: return new Hex(-1, 1);
                case Direction.Yz: return new Hex(0, 1);
                case Direction.Zx: return new Hex(-1, 0);
                case Direction.Zy: return new Hex(0, -1);

                case Direction.Xyz: return new Hex(2, -1);
                case Direction.xYz: return new Hex(-1, 2);
                case Direction.xyZ: return new Hex(-1, -1);
                case Direction.XYz: return new Hex(1, 1);
                case Direction.xYZ: return new Hex(-2, 1);
                case Direction.XyZ: return new Hex(1, -2);

                case Direction.Undefined:
                default: return new Hex(0, 0);
            }
        }

        /// <summary> Returns a <see cref="Hex"/> struct whose values are equal to this <see cref="Hex"/> plus a vector <see cref="Hex"/> via the given <see cref="Direction"/>. </summary>
        /// <param name="dir"> The <see cref="Direction"/> to use when calculating the <see cref="Hex"/> vector to add to this. </param>
        public Hex GetNeighborInDirection(Direction dir) => this + GetVectorFromDirection(dir);

        /// <summary> Returns a <see cref="Hex"/> struct whose value is equal to this <see cref="Hex"/> plus a vector <see cref="Hex"/> via the given <see cref="Direction"/>. 
        /// <para> Differs from <see cref="GetNeighborInDirection(Direction)"/> in that this method will return (0, 0) if given a non-adjacent <see cref="Direction"/>. </para></summary>
        /// <param name="dir"> The <see cref="Direction"/> in which to get an adjacent <see cref="Hex"/> struct. </param>
        public Hex GetAdjacentInDirection(Direction dir)
        {
            bool dirIsValid =
                (dir == Direction.Xy
                 || dir == Direction.Xz
                 || dir == Direction.Yx
                 || dir == Direction.Yz
                 || dir == Direction.Zx
                 || dir == Direction.Zy);

            return (dirIsValid) ? GetNeighborInDirection(dir) : Zero;
        }

        /// <summary> Returns the six <see cref="Hex"/> values which are adjacent to this one. </summary>
        public IEnumerable<Hex> GetAdjacents()
        {
            var ret = new List<Hex>
            {
                GetNeighborInDirection(Direction.Xy),
                GetNeighborInDirection(Direction.Xz),

                GetNeighborInDirection(Direction.Yx),
                GetNeighborInDirection(Direction.Yz),

                GetNeighborInDirection(Direction.Zx),
                GetNeighborInDirection(Direction.Zy),
            };

            return ret;
        }

        /// <summary> Returns a set of <see cref="Hex"/> values which make up an unbroken line from this <see cref="Hex"/> value to the given [endpoint] <see cref="Hex"/> value. </summary>
        /// <param name="endpoint"> The endpoint of the line to be calculated. </param>
        public List<Hex> GetLineToPoint(Hex endpoint)
        {
            int length = GetDistanceTo(endpoint);
            float step = 1.0f / Math.Max(length, 1);

            var ret = new List<Hex> {this};
            for (int i = 1; i <= length; i++)
            {
                float t = step * i;
                ret.Add(Lerp(this, endpoint, t));
            }

            return ret;
        }

        /// <summary> Returns the distance from this <see cref="Hex"/> value to the given <see cref="Hex"/> [target] value. </summary>
        /// <param name="target"> The <see cref="Hex"/> value for which a distance from this <see cref="Hex"/> will be calculated. </param>
        public int GetDistanceTo(Hex target) =>
            (Math.Abs(X - target.X) + Math.Abs(Y - target.Y) + Math.Abs(Z - target.Z)) / 2;


        /// <summary> Rotates this <see cref="Hex"/> coordinate 1 step (60°) in the given <see cref="RotationDirection"/> around a given [focal] point. </summary>
        /// <param name="focal"> The <see cref="Hex"/> point to rotate this <see cref="Hex"/> around. </param>
        /// <param name="dir"> The <see cref="RotationDirection"/> in which to rotate this <see cref="Hex"/> around the [focal] point.. </param>
        public Hex Rotate(Hex focal, RotationDirection dir) =>
            Rotate(focal, (dir == RotationDirection.Clockwise) ? 1 : -1);

        /// <summary> Rotates this <see cref="Hex"/> value around the given [focal] point by (steps * 60) degrees. </summary>
        /// <param name="focal"> The point which this <see cref="Hex"/> value will be rotated around. </param>
        /// <param name="steps"> The number of 60° rotation steps to take. Negative numbers are supported. </param>
        public Hex Rotate(Hex focal, int steps)
        {
            // determine direction!
            var rotationDirection = (steps > 0)
                ? RotationDirection.Clockwise
                : RotationDirection.CounterClockwise;

            // need to "zero out" the value first
            var rotating = this - focal;
            for (var stepsActual = Math.Abs(steps) % 6; stepsActual > 0; stepsActual--)
            {
                rotating = rotating.Rotate(rotationDirection);
            }

            // restore the value
            return (rotating + focal);
        }

        /// <summary> Rotates this <see cref="Hex"/> coordinate 60° in the given <see cref="RotationDirection"/> around the point (0, 0). </summary>
        /// <param name="dir"> The <see cref="RotationDirection"/> in which to rotate this <see cref="Hex"/> struct. </param>
        public Hex Rotate(RotationDirection dir)
        {
            switch (dir)
            {
                //    [ x , y , z ]
                // -> [-z ,-x ,-y ]
                case RotationDirection.Clockwise:
                    return new Hex(-Z, -X);

                //    [ x , y , z ] 
                // -> [-y ,-z ,-x ]
                case RotationDirection.CounterClockwise:
                    return new Hex(-Y, -Z);

                case RotationDirection.Undefined:
                default:
                    return this;
            }
        }

        /// <summary> Returns a set of <see cref="Hex"/> structs which contains every <see cref="Hex"/> value within the given distance of [radius] from this starting <see cref="Hex"/> value. </summary>
        /// <param name="radius"> The distance at which to find all <see cref="Hex"/> values within. </param>
        public IEnumerable<Hex> HexesInRadius(int radius)
        {
            var ret = new List<Hex>();
            for (int x = -radius; x <= radius; x++)
            {
                int yStart = Math.Max(-radius, -x - radius);
                int upperConstraint = Math.Min(radius, -x + radius);
                for (int y = yStart; y <= upperConstraint; y++)
                {
                    ret.Add(new Hex(X + x, Y + y));
                }
            }

            return ret;
        }

        /// <summary> Returns a set of <see cref="Hex"/> values which each are [distance] away from this starting <see cref="Hex"/> struct. </summary>
        /// <param name="distance"> The distance that each <see cref="Hex"/> returned will be from this starting <see cref="Hex"/> struct. </param>
        public IEnumerable<Hex> HexesAtDistance(int distance)
        {
            // Direction of rotation does not matter, so long as it's consistent.
            const RotationDirection rotDir = RotationDirection.Clockwise;

            // set up vars for stepping around the circumference (aka the 'ring')
            var currentVector = GetVectorFromDirection(Direction.Xz);
            var currentPosition = this + (currentVector * distance);

            // rotate vector to be aligned with side of ring correctly once loop starts.
            currentVector = currentVector.Rotate(rotDir);

            // loop once for each side of the hex
            var ret = new List<Hex>();
            for (int i = 0; i < 6; i++)
            {
                currentVector = currentVector.Rotate(rotDir);

                for (int j = 0; j < distance; j++)
                {
                    ret.Add(currentPosition);
                    currentPosition += currentVector;
                }
            }

            return ret;
        }


        /// <summary> Returns a set of <see cref="Hex"/> values with an <see cref="int"/> value which describes the distance from this starting <see cref="Hex"/> value they were found. </summary>
        /// <param name="maxDistance"> The maximum number of steps away from this starting coordinate that <see cref="Hex"/> values will be flood-filled into. </param>
        /// <param name="obstacles"> The set of <see cref="Hex"/> coordinates which are not allowed to be expanded into at any time. </param>
        public Dictionary<Hex, int> FloodFill(int maxDistance, List<Hex> obstacles = null) =>
            FloodFill(new[] {this}, maxDistance, obstacles);

        /// <summary> Returns a set of <see cref="Hex"/> values with an <see cref="int"/> value which describes the distance from this starting <see cref="Hex"/> value they were found. </summary>
        /// <param name="startHexes"> The set of starting <see cref="Hex"/> values from which to begin flood-filling. </param>
        /// <param name="maxDistance"> The maximum number of steps away from this starting coordinate that <see cref="Hex"/> values will be flood-filled into. </param>
        /// <param name="obstacles"> The set of <see cref="Hex"/> coordinates which are not allowed to be expanded into at any time. </param>
        public static Dictionary<Hex, int> FloodFill(IEnumerable<Hex> startHexes, int maxDistance,
            List<Hex> obstacles = null)
        {
            var obstaclesList = obstacles?.ToList() ?? new List<Hex>();
            var prev = new List<Hex>(startHexes);
            var ret = prev.ToDictionary(
                hex => hex,
                x => 0
            );

            for (int i = 1; i <= maxDistance; i++)
            {
                var current = new List<Hex>();
                foreach (var hex in prev)
                {
                    var adjacentHexes = hex.GetAdjacents();
                    foreach (var candidate in adjacentHexes)
                    {
                        if (ret.ContainsKey(candidate)) continue;
                        if (current.Contains(candidate)) continue;
                        if (obstaclesList.Contains(candidate)) continue;

                        current.Add(candidate);
                        ret.Add(candidate, i);
                    }
                }

                prev = current;
            }

            return ret;
        }


        /// <summary> Returns a <see cref="Hex"/> whose values are a transform of the difference of the two given <see cref="Hex"/> values, through the <see cref="Hex"/> value [a]. </summary>
        /// <param name="a"> The 'starting' <see cref="Hex"/> value for the interpolation. </param>
        /// <param name="b"> The 'ending' <see cref="Hex"/> value for the interpolation. </param>
        /// <param name="t"> The value to use as the transformation between the two [a] and [b] <see cref="Hex"/> structs. </param>
        public static Hex Lerp(Hex a, Hex b, float t) => a + (t * (b - a));

        /// <summary> Returns a new <see cref="Hex"/> struct whose values are equal to the sum of the two given <see cref="Hex"/> values. </summary>
        /// <param name="a"> The first of the two <see cref="Hex"/> structs to be added together. </param>
        /// <param name="b"> The second of the two <see cref="Hex"/> structs to be added together. </param>
        public static Hex operator +(Hex a, Hex b) => new Hex(a.X + b.X, a.Y + b.Y);

        /// <summary> Returns a new <see cref="Hex"/> struct whose value is equal to the given <see cref="Hex"/> [a] subtracted by the given <see cref="Hex"/> [b]. </summary>
        /// <param name="a"> The <see cref="Hex"/> whose values will be subtracted by [b]. </param>
        /// <param name="b"> The <see cref="Hex"/> whose value will be used to subtract [a] by. </param>
        public static Hex operator -(Hex a, Hex b) => new Hex(a.X - b.X, a.Y - b.Y);


        /// <summary> Returns a new <see cref="Hex"/> struct whose values equal the <see cref="Hex"/> struct's values multiplied by [t], and then rounded to the nearest integer.
        /// <para> Points to the [*] operator which accepts a <see cref="Hex"/> as the first parameter and a <see cref="float"/> value as the second. </para></summary>
        /// <param name="hex"> The <see cref="Hex"/> struct to multiply the values of by [t]. </param>
        /// <param name="t"> The value to multiply the given <see cref="Hex"/> struct's values by. </param>
        public static Hex operator *(float t, Hex hex) => hex * t;

        /// <summary> Returns a new <see cref="Hex"/> struct whose values equal the <see cref="Hex"/> struct's values multiplied by [t], and then rounded to the nearest integer. </summary>
        /// <param name="hex"> The <see cref="Hex"/> struct to multiply the values of by [t]. </param>
        /// <param name="t"> The value to multiply the given <see cref="Hex"/> struct's values by. </param>
        public static Hex operator *(Hex hex, float t) =>
            new Hex((int) Math.Round(hex.X * t), (int) Math.Round(hex.Y * t));

        /// <summary> Returns a new <see cref="Hex"/> struct whose values equal the <see cref="Hex"/> struct's values multiplied by [t].
        /// <para> Points to the [*] operator which accepts a <see cref="Hex"/> as the first parameter and a <see cref="int"/> value as the second. </para></summary>
        /// <param name="hex"> The <see cref="Hex"/> struct to multiply the values of by [t]. </param>
        /// <param name="t"> The value to multiply the given <see cref="Hex"/> struct's values by. </param>
        public static Hex operator *(int t, Hex hex) => hex * t;

        /// <summary> Returns a new <see cref="Hex"/> struct whose values equal the <see cref="Hex"/> struct's values multiplied by [t]. </summary>
        /// <param name="hex"> The <see cref="Hex"/> struct to multiply the values of by [t]. </param>
        /// <param name="t"> The value to multiply the given <see cref="Hex"/> struct's values by. </param>
        public static Hex operator *(Hex hex, int t) => new Hex(hex.X * t, hex.Y * t);


        /// <summary> Returns a <see cref="bool"/> value indicating whether the two given <see cref="Hex"/> structs are equal. </summary>
        /// <param name="a"> The first of the two <see cref="Hex"/> structs to be compared to each other. </param>
        /// <param name="b"> The second of the two <see cref="Hex"/> structs to be compared to each other. </param>
        public static bool operator ==(Hex a, Hex b)
        {
            return (a.X == b.X) && (a.Y == b.Y);
        }

        /// <summary> Returns a <see cref="bool"/> value indicating whether the two given <see cref="Hex"/> structs are NOT equal. </summary>
        /// <param name="a"> The first of the two <see cref="Hex"/> structs to be compared to each other. </param>
        /// <param name="b"> The second of the two <see cref="Hex"/> structs to be compared to each other. </param>
        public static bool operator !=(Hex a, Hex b)
        {
            return !(a == b);
        }


        /// <summary> Implicit conversion from a <see cref="Tuple"/>&lt;<see cref="int"/>, <see cref="int"/>&gt; to a <see cref="Hex"/> struct. </summary>
        /// <param name="tuple"> The <see cref="Tuple"/> to implicitly convert to a <see cref="Hex"/> struct. </param>
        public static implicit operator Hex((int X, int Y) tuple) => new Hex(tuple.X, tuple.Y);

        /// <summary> Returns a <see cref="string"/> representation of this <see cref="Hex"/> struct. </summary>
        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        /// <summary> Returns a <see cref="bool"/> value indicating whether the given [other] <see cref="Hex"/> is equal to this <see cref="Hex"/>.
        /// <para> Points to the '==' operator. </para></summary>
        /// <param name="other"> The other <see cref="Hex"/> struct to compare this <see cref="Hex"/> to. </param>
        public bool Equals(Hex other) => (this == other);

        /// <summary> Returns a <see cref="bool"/> value indicating whether the given [other] <see cref="object"/> is a <see cref="Hex"/> struct, and equal to this <see cref="Hex"/>. </summary>
        /// <param name="obj"> The <see cref="object"/> to check for equality to this <see cref="Hex"/> with. </param>
        public override bool Equals(object obj) => ((obj is Hex hex) && this == hex);

        /// <summary> Returns a HashCode for this <see cref="Hex"/> struct, based on it's X and Y values. </summary>
        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = (hashCode * -1521134295) + X.GetHashCode();
            hashCode = (hashCode * -1521134295) + Y.GetHashCode();
            return hashCode;
        }
    }
}