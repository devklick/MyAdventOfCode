﻿using MyAdventOfCode.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2015
{
    /// <summary>
    /// <see href="https://adventofcode.com/2015/day/2"/>
    /// </summary>
    public class D2
    {
        private readonly ITestOutputHelper _output;
        public D2(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Verifies the examples opn the page mentioned on the class comments.
        /// </summary>
        /// <param name="length">The length of the present</param>
        /// <param name="width">The width of the present</param>
        /// <param name="height">The height of the present</param>
        /// <param name="expectedRequiredPaper">The expected total paper required</param>
        [Theory]
        [InlineData(2, 3, 4, 58)]
        [InlineData(1, 1, 10, 43)]
        public void Part_1_VerifyExample(byte length, byte width, byte height, int expectedRequiredPaper)
        {
            var box = new PresentBox
            {
                Length = length,
                Width = width,
                Height = height
            };
            
            Assert.Equal(expectedRequiredPaper, box.TotalRequiredWrappingPaper);
        }

        /// <summary>
        /// Calculates the total paper required based on the <see cref="Presents"/>.
        /// </summary>
        [Fact]
        public void Part_1()
        {
            var total = Presents.Sum(p => p.TotalRequiredWrappingPaper);
            _output.WriteLine(total);
        }

        [Theory]
        [InlineData(2,3,4, 34)]
        [InlineData(1,1,10, 14)]
        public void Part_2_VerifyExample(byte length, byte width, byte height, int expectedRibbonLength)
        {
            var box = new PresentBox
            {
                Length = length,
                Width = width,
                Height = height
            };

            Assert.Equal(expectedRibbonLength, box.RibbonLength);
        }

        [Fact]
        public void Part_2()
        {
            var total = Presents.Sum(p => p.RibbonLength);
            _output.WriteLine(total);
        }

        /// <summary>
        /// Represents the box in which a present comes in.
        /// </summary>
        private class PresentBox
        {
            public byte Length { get; set; }
            public byte Width { get; set; }
            public byte Height { get; set; }

            /// <summary>
            /// The volume of the box, i.e: 
            /// <code><see cref="Length"/> * <see cref="Width"/> * <see cref="Height"/></code>
            /// </summary>
            public int Volume => Length * Width * Height;

            /// <summary>
            /// The surface area of the face made up by the length and height of the box, i.e:
            /// <code><see cref="Length"/> * <see cref="Height"/></code>
            /// </summary>
            public int LengthXHeightFaceSurfaceArea => Length * Height;

            /// <summary>
            /// The surface area of the face made up by the width and height of the box, i.e:
            /// <code><see cref="Width"/> * <see cref="Height"/></code>
            /// </summary>
            public int WidthXHeightFaceSurfaceArea => Width * Height;

            /// <summary>
            /// The surface area of the face made up by the width and length of the box, i.e:
            /// <code><see cref="Width"/> * <see cref="Length"/></code>
            /// </summary>
            public int WidthXLengthFaceSurfaceArea => Width * Length;

            /// <summary>
            /// The perimeter of the face made up by the length and height of the box, i.e.:
            /// <code>(<see cref="Length"/> + <see cref="Height"/>) * 2</code>
            /// </summary>
            public int LengthXHeightFacePerimeter => (Length + Height) * 2;

            /// <summary>
            /// The perimeter of the face made up by the width and height of the box, i.e.:
            /// <code>(<see cref="Width"/> + <see cref="Height"/>) * 2</code>
            /// </summary>
            public int WidthXHeightFacePerimeter => (Width + Height) * 2;
            
            /// <summary>
            /// The perimeter of the face made up by the width and length of the box, i.e.:
            /// <code>(<see cref="Width"/> + <see cref="Length"/>) * 2</code>
            /// </summary>
            public int WidthXLengthFacePerimeter => (Width + Length) * 2;

            /// <summary>
            /// The total surface area of all faces of the box, i.e. 
            /// <code>(<see cref="LengthXHeightFaceSurfaceArea"/> + <see cref="WidthXHeightFaceSurfaceArea"/> + <see cref="WidthXLengthFaceSurfaceArea"/>) * 2</code>
            /// </summary>
            public int TotalSurfaceArea => (LengthXHeightFaceSurfaceArea + WidthXHeightFaceSurfaceArea + WidthXLengthFaceSurfaceArea) * 2;

            /// <summary>
            /// The surface area of the smallest face.
            /// </summary>
            public int SmallestFaceSurfaceArea => new[] { LengthXHeightFaceSurfaceArea, WidthXHeightFaceSurfaceArea, WidthXLengthFaceSurfaceArea }.Min();
            
            /// <summary>
            /// The perimeter of the smallest face.
            /// </summary>
            public int SmallestFacePerimeter => new[] { LengthXHeightFacePerimeter, WidthXHeightFacePerimeter, WidthXLengthFacePerimeter }.Min();
            
            /// <summary>
            /// The total wrapping paper required to wrap the present, i.e:
            /// <code><see cref="TotalSurfaceArea"/> + <see cref="SmallestFaceSurfaceArea"/></code>
            /// </summary>
            public int TotalRequiredWrappingPaper => TotalSurfaceArea + SmallestFaceSurfaceArea;

            /// <summary>
            /// The length of ribbon required to decorate the box, i.e:
            /// <code><see cref="SmallestFacePerimeter"/> + <see cref="Volume"/></code>
            /// </summary>
            public int RibbonLength => SmallestFacePerimeter + Volume;
        }

        /// <summary>
        /// Splits the <see cref="DimensionsString"/> into a <see cref="List{T}"/> where <see cref="T"/> is a <see cref="PresentBox"/>.
        /// </summary>
        private List<PresentBox> Presents = DimensionsString
            .Split(Environment.NewLine)
            .Select(line =>
            {
                var split = line.Split("x");
                return new PresentBox
                {
                    Length = byte.Parse(split[0]),
                    Width = byte.Parse(split[1]),
                    Height = byte.Parse(split[2]),
                };
            }).ToList();

        private const string DimensionsString =
@"4x23x21
22x29x19
11x4x11
8x10x5
24x18x16
11x25x22
2x13x20
24x15x14
14x22x2
30x7x3
30x22x25
29x9x9
29x29x26
14x3x16
1x10x26
29x2x30
30x10x25
10x26x20
1x2x18
25x18x5
21x3x24
2x5x7
22x11x21
11x8x8
16x18x2
13x3x8
1x16x19
19x16x12
21x15x1
29x9x4
27x10x8
2x7x27
2x20x23
24x11x5
2x8x27
10x28x10
24x11x10
19x2x12
27x5x10
1x14x25
5x14x30
15x26x12
23x20x22
5x12x1
9x26x9
23x25x5
28x16x19
17x23x17
2x27x20
18x27x13
16x7x18
22x7x29
17x28x6
9x22x17
10x5x6
14x2x12
25x5x6
26x9x10
19x21x6
19x4x27
23x16x14
21x17x29
24x18x10
7x19x6
14x15x10
9x10x19
20x18x4
11x14x8
30x15x9
25x12x24
3x12x5
12x21x28
8x23x10
18x26x8
17x1x8
2x29x15
3x13x28
23x20x11
27x25x6
19x21x3
30x22x27
28x24x4
26x18x21
11x7x16
22x27x6
27x5x26
4x10x4
4x2x27
2x3x26
26x29x19
30x26x24
8x25x12
16x17x5
13x2x3
1x30x22
20x9x1
24x26x19
26x18x1
18x29x24
1x6x9
20x27x2
3x22x21
4x16x8
29x18x16
7x16x23
13x8x14
19x25x10
23x29x6
23x21x1
22x26x10
14x4x2
18x29x17
9x4x18
7x22x9
19x5x26
27x29x19
7x13x14
19x10x1
6x22x3
12x21x5
24x20x12
28x2x11
16x18x23
2x13x25
11x7x17
27x21x4
2x10x25
22x16x17
23x22x15
17x13x13
23x24x26
27x18x24
24x7x28
30x12x15
14x28x19
2x15x29
12x13x5
17x22x21
27x10x27
17x6x25
22x2x1
1x10x9
9x7x2
30x28x3
28x11x10
8x23x15
23x4x20
12x5x4
13x17x14
28x11x2
21x11x29
10x23x22
27x23x14
7x15x23
20x2x13
8x21x4
10x20x11
23x28x11
21x22x25
23x11x17
2x29x10
28x16x5
30x26x10
17x24x16
26x27x25
14x13x25
22x27x5
24x15x12
5x21x25
4x27x1
25x4x10
15x13x1
21x23x7
8x3x4
10x5x7
9x13x30
2x2x30
26x4x29
5x14x14
2x27x9
22x16x1
4x23x5
13x7x26
2x12x10
12x7x22
26x30x26
28x16x28
15x19x11
4x18x1
20x14x24
6x10x22
9x20x3
14x9x27
26x17x9
10x30x28
6x3x29
4x16x28
8x24x11
23x10x1
11x7x7
29x6x15
13x25x12
29x14x3
26x22x21
8x3x11
27x13x25
27x6x2
8x11x7
25x12x9
24x30x12
13x1x30
25x23x16
9x13x29
29x26x16
11x15x9
11x23x6
15x27x28
27x24x21
6x24x1
25x25x5
11x1x26
21x4x24
10x5x12
4x30x13
24x22x5
26x7x21
23x3x17
22x18x2
25x1x14
23x25x30
8x7x7
30x19x8
17x6x15
2x11x20
8x3x22
23x14x26
8x22x25
27x1x2
10x26x2
28x30x7
5x30x7
27x16x30
28x29x1
8x25x18
20x12x29
9x19x9
7x25x15
25x18x18
11x8x2
4x20x6
18x5x20
2x3x29
25x26x22
18x25x26
9x12x16
18x7x27
17x20x9
6x29x26
17x7x19
21x7x5
29x15x12
22x4x1
11x12x11
26x30x4
12x24x13
13x8x3
26x25x3
21x26x10
14x9x26
20x1x7
11x12x3
12x11x4
11x15x30
17x6x25
20x22x3
1x16x17
11x5x20
12x12x7
2x14x10
14x27x3
14x16x18
21x28x24
14x20x1
29x14x1
10x10x9
25x23x4
17x15x14
9x20x26
16x2x17
13x28x25
16x1x11
19x16x8
20x21x2
27x9x22
24x18x3
23x30x6
4x18x3
30x15x8
27x20x19
28x29x26
2x21x18
1x23x30
1x9x12
4x11x30
1x28x4
17x10x10
12x14x6
8x9x24
8x3x3
29x8x20
26x29x2
29x25x25
11x17x23
6x30x21
13x18x29
2x10x8
29x29x27
27x15x15
16x17x30
3x3x22
21x12x6
22x1x5
30x8x20
6x28x13
11x2x23
14x18x27
6x26x13
10x24x24
4x24x6
20x8x3
23x11x5
29x5x24
14x15x22
21x17x13
10x10x8
1x11x23
21x19x24
19x9x13
21x26x28
25x11x28
2x17x1
18x9x8
5x21x6
12x5x2
23x8x15
30x16x24
7x9x27
16x30x7
2x21x28
5x10x6
8x7x1
28x13x5
11x5x14
26x22x29
23x15x13
14x2x16
22x21x9
4x20x3
18x17x19
12x7x9
6x12x25
3x30x27
8x19x22
1x9x27
23x20x12
14x7x29
9x12x12
30x2x6
15x7x16
19x13x18
11x8x13
16x5x3
19x26x24
26x8x21
21x20x7
15x1x25
29x15x21
22x17x7
16x17x10
6x12x24
8x13x27
30x25x14
25x7x10
15x2x2
18x15x19
18x13x24
19x30x1
17x1x3
26x21x15
10x10x18
9x16x6
29x7x30
11x10x30
6x11x2
7x29x23
13x2x30
25x27x13
5x15x21
4x8x30
15x27x11
27x1x6
2x24x11
16x20x19
25x28x20
6x8x4
27x16x11
1x5x27
12x19x26
18x24x14
4x25x17
24x24x26
28x3x18
8x20x28
22x7x21
24x5x28
23x30x29
25x16x27
28x10x30
9x2x4
30x2x23
21x9x23
27x4x26
2x23x16
24x26x30
26x1x30
10x4x28
11x29x12
28x13x30
24x10x28
8x12x12
19x27x11
11x28x7
14x6x3
6x27x5
6x17x14
24x24x17
18x23x14
17x5x7
11x4x23
5x1x17
26x15x24
3x9x24
5x3x15
5x20x19
5x21x2
13x5x30
19x6x24
19x17x6
23x7x13
28x23x13
9x1x6
15x12x16
21x19x9
25x5x5
9x7x9
6x5x8
3x11x18
23x25x11
25x4x6
4x27x1
4x3x3
30x11x5
9x17x12
15x6x24
10x22x15
29x27x9
20x21x11
18x10x5
11x2x2
9x8x8
1x26x21
11x11x16
2x18x30
29x27x24
27x8x18
19x3x17
30x21x26
25x13x25
20x22x1
10x1x12
11x17x15
29x11x30
17x30x27
21x22x17
13x6x22
22x16x12
27x18x19
4x13x6
27x29x10
3x23x10
26x16x24
18x26x20
11x28x16
21x6x15
9x26x17
8x15x8
3x7x10
2x28x8
1x2x24
7x8x9
19x4x22
11x20x9
12x22x16
26x8x19
13x28x24
4x10x16
12x8x10
14x24x24
19x19x28
29x1x15
10x5x14
20x19x23
10x7x12
1x7x13
5x12x13
25x21x8
22x28x8
7x9x4
3x20x15
15x27x19
18x24x12
16x10x16
22x19x8
15x4x3
9x30x25
1x1x6
24x4x25
13x18x29
10x2x8
21x1x17
29x14x22
17x29x11
10x27x16
25x16x15
14x2x17
12x27x3
14x17x25
24x4x1
18x28x18
9x14x26
28x24x17
1x26x12
2x18x20
12x19x22
19x25x20
5x17x27
17x29x16
29x19x11
16x2x4
23x24x1
19x18x3
28x14x6
18x5x23
9x24x12
15x4x6
15x7x24
22x15x8
22x1x22
6x4x22
26x1x30
8x21x27
7x1x11
9x8x18
20x27x12
26x23x20
26x22x30
24x3x16
8x24x28
13x28x5
4x29x23
22x5x8
20x22x3
9x9x17
28x3x30
10x13x10
10x25x13
9x20x3
1x21x25
24x21x15
21x5x14
13x8x20
29x17x3
5x17x28
16x12x7
23x1x24
4x24x29
23x25x14
8x27x2
23x11x13
13x4x5
24x1x26
21x1x23
10x12x12
21x29x25
27x25x30
24x23x4
1x30x23
29x28x14
4x11x30
9x25x10
17x11x6
14x29x30
23x5x5
25x18x21
8x7x1
27x11x3
5x10x8
11x1x11
16x17x26
15x22x19
16x9x6
18x13x27
26x4x22
1x20x21
6x14x29
11x7x6
1x23x7
12x19x13
18x21x25
15x17x20
23x8x9
15x9x26
9x12x9
12x13x14
27x26x7
11x19x22
16x12x21
10x30x28
21x2x7
12x9x18
7x17x14
13x17x17
3x21x10
30x9x15
2x8x15
15x12x10
23x26x9
29x30x10
30x22x17
17x26x30
27x26x20
17x28x17
30x12x16
7x23x15
30x15x19
13x19x10
22x10x4
17x23x10
2x28x18
27x21x28
24x26x5
6x23x25
17x4x16
14x1x13
23x21x11
14x15x30
26x13x10
30x19x25
26x6x26
9x16x29
15x2x24
13x3x20
23x12x30
22x23x23
8x21x2
18x28x5
21x27x14
29x28x23
12x30x28
17x16x3
5x19x11
28x22x22
1x4x28
10x10x14
18x15x7
18x11x1
12x7x16
10x22x24
27x25x6
19x29x25
10x1x26
26x27x30
4x23x19
24x19x4
21x11x14
4x13x27
9x1x11
16x20x8
4x3x11
1x16x12
14x6x30
8x1x10
11x18x7
29x28x30
4x21x8
3x21x4
6x1x5
26x18x3
28x27x27
17x3x12
6x1x22
23x12x28
12x13x2
11x2x13
7x1x28
27x6x25
14x14x3
14x11x20
2x27x7
22x24x23
7x15x20
30x6x17
20x23x25
18x16x27
2x9x6
9x18x19
20x11x22
11x16x19
14x29x23
14x9x20
8x10x12
18x17x6
28x7x16
12x19x28
5x3x16
1x25x10
4x14x10
9x6x3
15x27x28
13x26x14
21x8x25
29x10x20
14x26x30
25x13x28
1x15x23
6x20x21
18x2x1
22x25x16
23x25x17
2x14x21
14x25x16
12x17x6
19x29x15
25x9x6
19x17x13
24x22x5
19x4x13
10x18x6
6x25x6
23x24x20
8x22x13
25x10x29
5x12x25
20x5x11
7x16x29
29x24x22
28x20x1
10x27x10
6x9x27
26x15x30
26x3x19
20x11x3
26x1x29
6x23x4
6x13x21
9x23x25
15x1x10
29x12x13
7x8x24
29x30x27
3x29x19
14x16x17
4x8x27
26x17x8
10x27x17
11x28x17
17x16x27
1x8x22
6x30x16
7x30x22
20x12x3
18x10x2
20x21x26
11x1x17
9x15x15
19x14x30
24x22x20
11x26x23
14x3x23
1x28x29
29x20x4
1x4x20
12x26x8
14x11x14
14x19x13
15x13x24
16x7x26
11x20x11
5x24x26
24x25x7
21x3x14
24x29x20
7x12x1
16x17x4
29x16x21
28x8x17
11x30x25
1x26x23
25x19x28
30x24x5
26x29x15
4x25x23
14x25x19
29x10x7
29x29x28
19x13x24
21x28x5
8x15x24
1x10x12
2x26x6
14x14x4
10x16x27
9x17x25
25x8x7
1x9x28
10x8x17
4x12x1
17x26x29
23x12x26
2x21x22
18x23x13
1x14x5
25x27x26
4x30x30
5x13x2
17x9x6
28x18x28
7x30x2
28x22x17
14x15x14
10x14x19
6x15x22
27x4x17
28x21x6
19x29x26
6x17x17
20x13x16
25x4x1
2x9x5
30x3x1
24x21x2
14x19x12
22x5x23
14x4x21
10x2x17
3x14x10
17x5x3
22x17x13
5x19x3
29x22x6
12x28x3
9x21x25
10x2x14
13x26x7
18x23x2
9x14x17
21x3x13
13x23x9
1x20x4
11x4x1
19x5x30
9x9x29
26x29x14
1x4x10
7x27x30
8x3x23
1x27x27
7x27x27
1x26x16
29x16x14
18x6x12
24x24x24
26x2x19
15x17x4
11x7x14
14x19x10
9x10x1
14x17x9
20x19x13
25x20x8
24x20x21
26x30x2
24x2x10
28x4x13
27x17x11
15x3x8
11x29x10
26x15x16
4x28x22
7x5x22
10x28x9
6x28x13
10x5x6
20x12x6
25x30x30
17x16x14
14x20x3
16x10x8
9x28x14
16x12x12
11x13x25
21x16x28
10x3x18
5x9x20
17x23x5
3x13x16
29x30x17
2x2x8
15x8x30
20x1x16
23x10x29
4x5x4
6x18x12
26x10x22
21x10x17
26x12x29
7x20x21
18x9x15
10x23x20
20x1x27
10x10x3
25x12x23
30x11x15
16x22x3
22x10x11
15x10x20
2x20x17
20x20x1
24x16x4
23x27x7
7x27x22
24x16x8
20x11x25
30x28x11
21x6x24
15x2x9
16x30x24
21x27x9
7x19x8
24x13x28
12x26x28
16x21x11
25x5x13
23x3x17
23x1x17
4x17x18
17x13x18
25x12x19
17x4x19
4x21x26
6x28x1
23x22x15
6x23x12
21x17x9
30x4x23
2x19x21
28x24x7
19x24x14
13x20x26
19x24x29
8x26x3
16x12x14
17x4x21
8x4x20
13x27x17
9x21x1
29x25x6
7x9x26
13x25x5
6x9x21
12x10x11
30x28x21
15x6x2
8x18x19
26x20x24
26x17x14
27x8x1
19x19x18
25x24x27
14x29x15
22x26x1
14x17x9
2x6x23
29x7x5
14x16x19
14x21x18
10x15x23
21x29x14
20x29x30
23x11x5";

    }
}
