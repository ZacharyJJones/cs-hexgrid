using System;
using System.Collections.Generic;
using System.Linq;

namespace HexGrid
{
  public enum HexDirection
  {
    /// <summary> Obligatory 'Undefined' enumeration value. </summary>
    Undefined = 0,

    /// <summary> (1, 0) </summary>
    SideXPos,
    /// <summary> (-1, 0) </summary>
    SideXNeg,

    /// <summary> (0, 1) </summary>
    SideYPos,
    /// <summary> (0, -1) </summary>
    SideYNeg,

    /// <summary> (1, -1) </summary>
    SideZPos,
    /// <summary> (-1, 1) </summary>
    SideZNeg,


    /// <summary> (1, 1) </summary>
    DiagXPos,
    /// <summary> (-1, -1) </summary>
    DiagXNeg,

    /// <summary> (-1, 2) </summary>
    DiagYPos,
    /// <summary> (1, -2) </summary>
    DiagYNeg,

    /// <summary> (2, -1) </summary>
    DiagZPos,
    /// <summary> (-2, 1) </summary>
    DiagZNeg
  }

  public enum RotationDirection
  {
    /// <summary> Obligatory 'Undefined' enumeration value. </summary>
    Undefined = 0,

    Clockwise,
    CounterClockwise
  }

  public readonly struct Hex : IEquatable<Hex>
  {
    /// <summary> Pre-computed value for sqrt(3)/2. Used for converting hexagonal coordinates to 2d. </summary>
    private const float SQRT_3o2 = 0.866025f;

    /// <summary> The X-component of this Hex value. </summary>
    public readonly int X;

    /// <summary> The Y-component of this Hex value. </summary>
    public readonly int Y;

    /// <summary> The Z-component of this Hex value. </summary>
    public readonly int Z;


    public Hex(int x, int y)
    {
      X = x;
      Y = y;

      Z = -x - y;
    }


    /// <summary> Returns a new Hex value whose X and Y values are equal to 0. </summary>
    public static Hex Zero => new Hex(0, 0);

    /// <summary> Returns a new Hex whose X and Y values are equal to 1. </summary>
    public static Hex One => new Hex(1, 1);

    public (int X, int Y) AsTuple => (X, Y);

    public (float X, float Y) InTwoDSpace => (X + 0.5f * Y, SQRT_3o2 * Y);

    public static Hex FromTwoDSpace(float x, float y)
    {
      // x = coordX + 0.5f * coordY
      // y = coordY * SQRT_3o2

      // coordY = y / SQRT_3o2
      // coordX = x - 0.5f * coordY

      // Math should be correct...
      int hexY = (int)Math.Round(y / SQRT_3o2);
      int hexX = (int)Math.Round(x - 0.5f * hexY);
      return new Hex(hexX, hexY);
    }

    /// <summary> Returns the six Hex values which are adjacent to this one. </summary>
    public List<Hex> Adjacents
    {
      get
      {
        var ret = new List<Hex>
        {
          GetNeighbor(HexDirection.SideXNeg),
          GetNeighbor(HexDirection.SideXPos),

          GetNeighbor(HexDirection.SideYPos),
          GetNeighbor(HexDirection.SideYNeg),

          GetNeighbor(HexDirection.SideZPos),
          GetNeighbor(HexDirection.SideZNeg),
        };

        return ret;
      }
    }

    // is "GetNeighbor" and not "GetAdjacent" because it allows for all HexDirection values.
    public Hex GetNeighbor(HexDirection dir) => this + GetVectorFromDirection(dir);

    /// <summary> Returns a <see cref="Hex"/> vector value based on the given <see cref="HexDirection"/>. </summary>
    /// <param name="dir"> The <see cref="HexDirection"/> to acquire a <see cref="Hex"/> vector for. </param>
    public static Hex GetVectorFromDirection(HexDirection dir)
    {
      switch (dir)
      {
        case HexDirection.SideXPos: return new Hex(1, 0);
        case HexDirection.SideXNeg: return new Hex(-1, 0);
        case HexDirection.SideYPos: return new Hex(0, 1);
        case HexDirection.SideYNeg: return new Hex(0, -1);
        case HexDirection.SideZPos: return new Hex(1, -1);
        case HexDirection.SideZNeg: return new Hex(-1, 1);

        case HexDirection.DiagXPos: return new Hex(1, 1);
        case HexDirection.DiagXNeg: return new Hex(-1, -1);
        case HexDirection.DiagYPos: return new Hex(-1, 2);
        case HexDirection.DiagYNeg: return new Hex(1, -2);
        case HexDirection.DiagZPos: return new Hex(2, -1);
        case HexDirection.DiagZNeg: return new Hex(-2, 1);

        case HexDirection.Undefined:
        default: return new Hex(0, 0);
      }
    }

    /// <summary> Returns the distance from this Hex to the [target] Hex. </summary>
    /// <param name="target"> The Hex to get a distance to. </param>
    public int DistanceTo(Hex target)
    {
      return (
        Math.Abs(X - target.X) +
        Math.Abs(Y - target.Y) +
        Math.Abs(Z - target.Z)
      ) / 2;
    }

    /// <summary> Returns a set of Hex values which make up an unbroken line from this Hex to the given [endpoint]. </summary>
    /// <param name="endpoint"> The inclusive endpoint of the line to be calculated. </param>
    public List<Hex> GetLineToPoint(Hex endpoint)
    {
      int length = DistanceTo(endpoint);
      float step = 1.0f / Math.Max(length, 1);

      var ret = new List<Hex> { this };
      for (int i = 1; i <= length; i++)
      {
        float t = step * i;
        ret.Add(Lerp(this, endpoint, t));
      }

      return ret;
    }

    /// <summary> Rotates this Hex coordinate 1 step (60 degrees) in the given direction around the point (0,0). </summary>
    /// <param name="dir"> The direction in which to rotate. </param>
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

    /// <summary> Rotates this Hex coordinate about the given [focal] point by [steps] increments of 60 degrees. </summary>
    /// <param name="focal"> The point which this Hex value will be rotated around. </param>
    /// <param name="steps"> The number of 60° rotation steps to take. Negative numbers are supported. </param>
    public Hex Rotate(Hex focal, int steps)
    {
      // determine direction!
      var rotationDirection = (steps > 0)
          ? RotationDirection.Clockwise
          : RotationDirection.CounterClockwise;

      // Rotation mathematically occurs around (0,0)
      // ... subtracting the focal point provides correct behavior.
      var rotating = this - focal;

      for (var stepsActual = Math.Abs(steps) % 6; stepsActual > 0; stepsActual--)
      {
        rotating = rotating.Rotate(rotationDirection);
      }

      // restore the value
      return (rotating + focal);
    }

    /// <summary> Rotates this Hex coordinate 1 step (60 degrees) in the given direction around a given [focal] point. </summary>
    /// <param name="focal"> The Hex point to rotate this value about. </param>
    /// <param name="dir"> The direction in which to rotate. </param>
    public Hex Rotate(Hex focal, RotationDirection dir)
    {
      return Rotate(focal, (dir == RotationDirection.Clockwise) ? 1 : -1);
    }

    /// <summary> Returns a list of Hex values which are at or within a distance of [radius] from this Hex coordinate. Includes self. </summary>
    /// <param name="radius"> The max distance from this coordinate at which to calculate Hex values. </param>
    public List<Hex> HexesInRadius(int radius)
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

    /// <summary> Returns a "ring" of Hex values which each are [distance] away from this starting coordinate. </summary>
    public List<Hex> HexesAtRadius(int radius)
    {
      // set up vars for stepping around the circumference (aka the 'ring')
      var currentVector = GetVectorFromDirection(HexDirection.SideXPos);
      var currentPosition = this + (currentVector * radius);

      // rotate vector to be aligned with side of ring correctly once loop starts.
      currentVector = currentVector.Rotate(RotationDirection.Clockwise);

      // loop once for each side of the hex
      var ret = new List<Hex>();
      for (int i = 0; i < 6; i++)
      {
        currentVector = currentVector.Rotate(RotationDirection.Clockwise);

        for (int j = 0; j < radius; j++)
        {
          ret.Add(currentPosition);
          currentPosition += currentVector;
        }
      }

      return ret;
    }

    /// <summary> Flood-Fill using this Hex coordinate as the single startHex.  </summary>
    /// <param name="maxDistance"> The maximum number of steps from this Hex coordinate that values will flood-fill into. </param>
    /// <param name="obstacles"> The set of Hex coordinates which are not allowed to be expanded into at any time. </param>
    public List<List<Hex>> FloodFill(int maxDistance, List<Hex> obstacles = null)
    {
      return FloodFill(new[] { this }, maxDistance, obstacles);
    }

    /// <param name="startHexes"> The set of starting Hex values from which to begin flood-filling. </param>
    /// <param name="maxDistance"> The maximum number of steps away from this starting coordinate that Hex values will be flood-filled into. </param>
    /// <param name="obstacles"> The set of Hex coordinates which are not allowed to be flood-filled into at any time. </param>
    public static List<List<Hex>> FloodFill(IEnumerable<Hex> startHexes, int maxDistance, IEnumerable<Hex> obstacles = null)
    {
      obstacles = obstacles?.ToList() ?? new List<Hex>();
      var ret = new List<List<Hex>>();
      ret.Add(startHexes.ToList());

      for (int distance = 1; distance <= maxDistance; distance++)
      {
        // Can stop early if there's nothing to fill from.
        if (ret[distance - 1].Count == 0) break;

        ret.Add(new List<Hex>());
        foreach (var hex in ret[distance - 1])
        {
          foreach (var candidate in hex.Adjacents)
          {
            if (distance >= 2 && ret[distance - 2].Contains(candidate)) continue;
            if (ret[distance - 1].Contains(candidate)) continue;
            if (ret[distance].Contains(candidate)) continue;
            if (obstacles.Contains(candidate)) continue;

            ret[distance].Add(candidate);
          }
        }
      }

      return ret;
    }




    /// <summary> Returns a Hex coordinate whose values are a linear interpolation of [t] between the Hexes [a] and [b]. </summary>
    /// <param name="a"> The 'starting' Hex value for the interpolation. </param>
    /// <param name="b"> The 'ending' Hex value for the interpolation. </param>
    /// <param name="t"> The interpolant value. </param>
    public static Hex Lerp(Hex a, Hex b, float t) => a + (t * (b - a));



    public static Hex operator +(Hex a, Hex b) => new Hex(a.X + b.X, a.Y + b.Y);

    public static Hex operator -(Hex a, Hex b) => new Hex(a.X - b.X, a.Y - b.Y);

    public static Hex operator -(Hex hex) => new Hex(-hex.X, -hex.Y);

    /// <summary> Multiplies input <see cref="Hex"/> X and Y values by t and returns as a new Hex, rounded to nearest integers. </summary>
    public static Hex operator *(float t, Hex hex) => hex * t;
    /// <summary> Multiplies input <see cref="Hex"/> X and Y values by t and returns as a new Hex, rounded to nearest integers. </summary>
    public static Hex operator *(Hex hex, float t)
    {
      return new Hex(
        (int)Math.Round(hex.X * t),
        (int)Math.Round(hex.Y * t)
      );
    }

    /// <summary> Multiplies input <see cref="Hex"/> X and Y values by t and returns as a new Hex. </summary>
    public static Hex operator *(int t, Hex hex) => hex * t;

    /// <summary> Multiplies input <see cref="Hex"/> X and Y values by t and returns as a new Hex. </summary>
    public static Hex operator *(Hex hex, int t) => new Hex(hex.X * t, hex.Y * t);

    public static bool operator ==(Hex a, Hex b) => (a.X == b.X) && (a.Y == b.Y);

    public static bool operator !=(Hex a, Hex b) => !(a == b);

    public static implicit operator Hex((int X, int Y) tuple) => new Hex(tuple.X, tuple.Y);



    public override string ToString() => $"({X}, {Y})";

    bool IEquatable<Hex>.Equals(Hex other) => (this == other);

    public override bool Equals(object obj) => ((obj is Hex hex) && this == hex);

    public override int GetHashCode()
    {
      var hashCode = 1861411795;
      hashCode = (hashCode * -1521134295) + X.GetHashCode();
      hashCode = (hashCode * -1521134295) + Y.GetHashCode();
      return hashCode;
    }
  }
}