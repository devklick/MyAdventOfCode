using MyAdventOfCode.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace MyAdventOfCode.Y2015
{
    /// <summary>
    /// Wires and bitwise logic gates.
    /// See <see href="https://adventofcode.com/2015/day/7"/>
    /// </summary>
    public class D7
    {
        private readonly ITestOutputHelper _output;

        public D7(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Part_1_Example_Parse1()
        {
            var input = "123 -> x";
            var result = Gate.Parse(input);
            var direct = Assert.IsType<Direct>(result);
            Assert.Single(direct.InputValues);
            Assert.NotNull(direct.InputValues.First().SignalValue);
            Assert.Equal(123, direct.InputValues.First().SignalValue.Value);
            Assert.Null(direct.InputValues.First().SignalSourceWireId);
            Assert.Equal("x", direct.TargetWireId);
        }

        [Fact]
        public void Part_1_Example_Parse2()
        {
            var input = "456 -> y";
            var result = Gate.Parse(input);
            var direct = Assert.IsType<Direct>(result);
            Assert.Single(direct.InputValues);
            Assert.NotNull(direct.InputValues.First().SignalValue);
            Assert.Equal(456, direct.InputValues.First().SignalValue.Value);
            Assert.Null(direct.InputValues.First().SignalSourceWireId);
            Assert.Equal("y", direct.TargetWireId);
        }

        [Fact]
        public void Part_1_Example_Parse3()
        {
            var input = "x AND y -> d";
            var result = Gate.Parse(input);
            var and = Assert.IsType<And>(result);
            Assert.Equal(2, and.InputValues.Count());
            Assert.Null(and.InputValues.First().SignalValue);
            Assert.Equal("x", and.InputValues.First().SignalSourceWireId);
            Assert.Null(and.InputValues.Last().SignalValue);
            Assert.Equal("y", and.InputValues.Last().SignalSourceWireId);
            Assert.Equal("d", and.TargetWireId);
        }

        [Fact]
        public void Part_1_Example_Parse4()
        {
            var input = "x OR y -> e";
            var result = Gate.Parse(input);
            var or = Assert.IsType<Or>(result);
            Assert.Equal(2, or.InputValues.Count());
            Assert.Null(or.InputValues.First().SignalValue);
            Assert.Equal("x", or.InputValues.First().SignalSourceWireId);
            Assert.Null(or.InputValues.Last().SignalValue);
            Assert.Equal("y", or.InputValues.Last().SignalSourceWireId);
            Assert.Equal("e", or.TargetWireId);
        }

        [Fact]
        public void Part_1_Example_Parse5()
        {
            var input = "x LSHIFT 2 -> f";
            var result = Gate.Parse(input);
            var shift = Assert.IsType<LeftShift>(result);
            Assert.Equal(2, shift.InputValues.Count());
            Assert.Null(shift.InputValues.First().SignalValue);
            Assert.Equal("x", shift.InputValues.First().SignalSourceWireId);
            Assert.NotNull(shift.InputValues.Last().SignalValue);
            Assert.Equal(2, shift.InputValues.Last().SignalValue.Value);
            Assert.Null(shift.InputValues.Last().SignalSourceWireId);
            Assert.Equal("f", shift.TargetWireId);
        }

        [Fact]
        public void Part_1_Example_Parse6()
        {
            var input = "y RSHIFT 2 -> g";
            var result = Gate.Parse(input);
            var shift = Assert.IsType<RightShift>(result);
            Assert.Equal(2, shift.InputValues.Count());
            Assert.Null(shift.InputValues.First().SignalValue);
            Assert.Equal("y", shift.InputValues.First().SignalSourceWireId);
            Assert.NotNull(shift.InputValues.Last().SignalValue);
            Assert.Equal(2, shift.InputValues.Last().SignalValue.Value);
            Assert.Null(shift.InputValues.Last().SignalSourceWireId);
            Assert.Equal("g", shift.TargetWireId);
        }

        [Fact]
        public void Part_1_Example_Parse7()
        {
            var input = "NOT x -> h";
            var result = Gate.Parse(input);
            var comp = Assert.IsType<Complement>(result);
            Assert.Single(comp.InputValues);
            Assert.Equal("x", comp.InputValues.First().SignalSourceWireId);
            Assert.Null(comp.InputValues.First().SignalValue);
            Assert.Equal("h", comp.TargetWireId);
        }

        [Fact]
        public void Part_1_Example_Parse8()
        {
            var input = "NOT y -> i";
            var result = Gate.Parse(input);
            var comp = Assert.IsType<Complement>(result);
            Assert.Single(comp.InputValues);
            Assert.Equal("y", comp.InputValues.First().SignalSourceWireId);
            Assert.Null(comp.InputValues.First().SignalValue);
            Assert.Equal("i", comp.TargetWireId);
        }

        [Fact]
        public void Part_1_FullExample()
        {
            var input =
@"123 -> x
456 -> y
x AND y -> d
x OR y -> e
x LSHIFT 2 -> f
y RSHIFT 2 -> g
NOT x -> h
NOT y -> i";

            var circuit = new Circuit();

            foreach (var gate in Gate.ParseMultiple(input))
            {
                circuit.ProcessGate(gate);
            }

            Assert.Equal(72, circuit["d"].Signal);
            Assert.Equal(507, circuit["e"].Signal);
            Assert.Equal(492, circuit["f"].Signal);
            Assert.Equal(114, circuit["g"].Signal);
            Assert.Equal(65412, circuit["h"].Signal);
            Assert.Equal(65079, circuit["i"].Signal);
            Assert.Equal(123, circuit["x"].Signal);
            Assert.Equal(456, circuit["y"].Signal);
        }

        [Fact]
        public void Part_1()
        {
            var circuit = new Circuit();

            var gates = Gate.ParseMultiple(Input);

            circuit.ProcessGates(gates);

            _output.WriteLine(circuit["a"].Signal);
        }

        [Fact]
        public void Part_2()
        {
            var circuit = new Circuit();

            var gates = Gate.ParseMultiple(Input).ToList();
            
            // This is the result from part 1
            gates.Add(Gate.Parse("16076 -> b"));

            circuit.ProcessGates(gates);

            _output.WriteLine(circuit["a"].Signal);
        }


        private class Wire
        {
            public string Id { get; set; }
            public ushort Signal { get; set; }
        }

        private enum Operator
        {
            DirectAssign = 0,
            AND,
            OR,
            LSHIFT,
            RSHIFT,
            NOT
        }

        private abstract class Gate : IValidatable
        {
            public abstract Operator Operator { get; }
            public abstract int ValidValuesCountMin { get; }
            public abstract int ValidValuesCountMax { get; }
            public string TargetWireId { get; set; }
            public List<InputValue> InputValues { get; set; }
            public virtual bool IsValid => TargetWireId != null 
                && InputValues != null
                && InputValues.Count >= ValidValuesCountMin
                && InputValues.Count <= ValidValuesCountMax;

            public static IEnumerable<Gate> ParseMultiple(string input)
            {
                foreach (var part in input.Split(Environment.NewLine))
                {
                    yield return Parse(part);
                }
            }

            public static Gate Parse(string input)
            {
                var parts = input.Split(" ");
                var target = parts.Last();
                var operators = new List<Operator>();
                var values = new List<InputValue>();
                var operatorType = typeof(Operator);

                foreach (var part in parts.SkipLast(1))
                {
                    if (ushort.TryParse(part, out var u))
                    {
                        values.Add(new InputValue
                        {
                            SignalValue = u
                        });
                    }
                    else if (Enum.TryParse<Operator>(part, out var o)
                        && Enum.IsDefined(operatorType, o))
                    {
                        operators.Add(o);
                    }
                    else if (part != "->")
                    {
                        values.Add(new InputValue
                        {
                            SignalSourceWireId = part
                        });
                    }
                }

                if (operators.Count > 1)
                    throw new InvalidOperationException("Multiple operators specified, but only a single operator is currently supported.");
                
                var op = operators.FirstOrDefault();

                Gate gate = op switch
                {
                    Operator.DirectAssign => new Direct { InputValues = values, TargetWireId = target },
                    Operator.AND => new And { InputValues = values, TargetWireId = target },
                    Operator.OR => new Or { InputValues = values, TargetWireId = target },
                    Operator.LSHIFT => new LeftShift { InputValues = values, TargetWireId = target },
                    Operator.RSHIFT => new RightShift { InputValues = values, TargetWireId = target },
                    Operator.NOT => new Complement { InputValues = values, TargetWireId = target },
                    _ => throw new NotImplementedException(),
                };

                if (!gate.IsValid)
                    throw new InvalidOperationException($"The instruction for operator {gate.Operator} was invalid.");

                return gate;
            }

            protected void ValidateSignalCount(IEnumerable<ushort> signals)
            {
                if (signals == null
                    || signals.Count() < ValidValuesCountMin
                    || signals.Count() > ValidValuesCountMax)
                {
                    throw new InvalidOperationException(
                        $"Expected between {ValidValuesCountMin} and {ValidValuesCountMax} signals but got {signals.Count()}.");
                }
            }

            public abstract void ApplySignal(Wire target, IEnumerable<ushort> signals);
        }

        private class Direct : Gate
        {
            public override Operator Operator => Operator.DirectAssign;
            public override int ValidValuesCountMin => 1;
            public override int ValidValuesCountMax => 1;

            public override void ApplySignal(Wire target, IEnumerable<ushort> signals)
            {
                ValidateSignalCount(signals);

                target.Signal = signals.First();
            }
        }

        private class And : Gate
        {
            public override int ValidValuesCountMin => 2;
            public override int ValidValuesCountMax => 2;
            public override Operator Operator => Operator.AND;

            public override void ApplySignal(Wire target, IEnumerable<ushort> signals)
            {
                ValidateSignalCount(signals);

                target.Signal = (ushort)(signals.First() & signals.Last());
            }
        }

        private class Or : Gate
        {
            public override Operator Operator => Operator.OR;
            public override int ValidValuesCountMin => 2;
            public override int ValidValuesCountMax => 2;

            public override void ApplySignal(Wire target, IEnumerable<ushort> signals)
            {
                ValidateSignalCount(signals);

                target.Signal = (ushort)(signals.First() | signals.Last());
            }
        }

        private class RightShift : Gate
        {
            public override int ValidValuesCountMin => 2;
            public override int ValidValuesCountMax => 2;
            public override Operator Operator => Operator.RSHIFT;

            public override void ApplySignal(Wire target, IEnumerable<ushort> signals)
            {
                ValidateSignalCount(signals);

                target.Signal = (ushort)(signals.First() >> signals.Last());
            }
        }

        private class LeftShift : Gate
        {
            public override int ValidValuesCountMin => 2;
            public override int ValidValuesCountMax => 2;
            public override Operator Operator => Operator.LSHIFT;

            public override void ApplySignal(Wire target, IEnumerable<ushort> signals)
            {
                ValidateSignalCount(signals);

                target.Signal = (ushort)(signals.First() << signals.Last());
            }
        }

        private class Complement : Gate
        {
            public override int ValidValuesCountMin => 1;
            public override int ValidValuesCountMax => 1;
            public override Operator Operator => Operator.NOT;

            public override void ApplySignal(Wire target, IEnumerable<ushort> signals)
            {
                ValidateSignalCount(signals);

                target.Signal = (ushort)~signals.Last();
            }
        }

        private interface IValidatable
        {
            bool IsValid { get; }
        }

        /// <summary>
        /// Inputs can be specified as either a raw value, or a reference to an 
        /// existing wire who's value should be used as the input.
        /// </summary>
        private class InputValue
        {
            /// <summary>
            /// The raw signal value to be used for input, if applicable.
            /// </summary>
            public ushort? SignalValue { get; set; }

            /// <summary>
            /// The ID of the wire who's signal should be used as the input, if applicable.
            /// </summary>
            public string SignalSourceWireId { get; set; }
        }

        private class Circuit : Dictionary<string, Wire>
        {

            public void ProcessGates(IEnumerable<Gate> gates)
            {
                foreach(var batch in GetNextBatch(gates))
                {
                    foreach(var gate in batch)
                    {
                        ProcessGate(gate);
                    }
                }
            }

            public IEnumerable<IEnumerable<Gate>> GetNextBatch(IEnumerable<Gate> gates)
            {
                var fullset = gates.ToList();

                while (fullset.Any())
                {
                    var batch = fullset.Where(gate => 
                        gate.InputValues.All(value => 
                            (value.SignalSourceWireId == null && value.SignalValue.HasValue) 
                            || (value.SignalSourceWireId != null && ContainsKey(value.SignalSourceWireId)))).ToList();

                    batch.ForEach(b => fullset.Remove(b));
                    yield return batch;
                }
            }

            public void ProcessGate(Gate gate)
            {
                if (!TryGetValue(gate.TargetWireId, out var target))
                {
                    target = new Wire
                    {
                        Id = gate.TargetWireId,
                        Signal = 0
                    };
                    Add(target.Id, target);
                }

                var signals = new List<ushort>();

                foreach (var input in gate.InputValues)
                {
                    if (input.SignalValue.HasValue)
                    {
                        signals.Add(input.SignalValue.Value);
                    }
                    else if (input.SignalSourceWireId != null)
                    {
                        if (!TryGetValue(input.SignalSourceWireId, out var source))
                        {
                            source = new Wire
                            {
                                Id = input.SignalSourceWireId,
                                Signal = 0
                            };
                            Add(source.Id, source);
                        }
                        signals.Add(this[input.SignalSourceWireId].Signal);
                    }
                    else throw new InvalidOperationException("No input signal value or source wire Id specified");
                }

                gate.ApplySignal(target, signals);
            }
        }

        private const string Input =
@"lf AND lq -> ls
iu RSHIFT 1 -> jn
bo OR bu -> bv
gj RSHIFT 1 -> hc
et RSHIFT 2 -> eu
bv AND bx -> by
is OR it -> iu
b OR n -> o
gf OR ge -> gg
NOT kt -> ku
ea AND eb -> ed
kl OR kr -> ks
hi AND hk -> hl
au AND av -> ax
lf RSHIFT 2 -> lg
dd RSHIFT 3 -> df
eu AND fa -> fc
df AND dg -> di
ip LSHIFT 15 -> it
NOT el -> em
et OR fe -> ff
fj LSHIFT 15 -> fn
t OR s -> u
ly OR lz -> ma
ko AND kq -> kr
NOT fx -> fy
et RSHIFT 1 -> fm
eu OR fa -> fb
dd RSHIFT 2 -> de
NOT go -> gp
kb AND kd -> ke
hg OR hh -> hi
jm LSHIFT 1 -> kg
NOT cn -> co
jp RSHIFT 2 -> jq
jp RSHIFT 5 -> js
1 AND io -> ip
eo LSHIFT 15 -> es
1 AND jj -> jk
g AND i -> j
ci RSHIFT 3 -> ck
gn AND gp -> gq
fs AND fu -> fv
lj AND ll -> lm
jk LSHIFT 15 -> jo
iu RSHIFT 3 -> iw
NOT ii -> ij
1 AND cc -> cd
bn RSHIFT 3 -> bp
NOT gw -> gx
NOT ft -> fu
jn OR jo -> jp
iv OR jb -> jc
hv OR hu -> hw
19138 -> b
gj RSHIFT 5 -> gm
hq AND hs -> ht
dy RSHIFT 1 -> er
ao OR an -> ap
ld OR le -> lf
bk LSHIFT 1 -> ce
bz AND cb -> cc
bi LSHIFT 15 -> bm
il AND in -> io
af AND ah -> ai
as RSHIFT 1 -> bl
lf RSHIFT 3 -> lh
er OR es -> et
NOT ax -> ay
ci RSHIFT 1 -> db
et AND fe -> fg
lg OR lm -> ln
k AND m -> n
hz RSHIFT 2 -> ia
kh LSHIFT 1 -> lb
NOT ey -> ez
NOT di -> dj
dz OR ef -> eg
lx -> a
NOT iz -> ja
gz LSHIFT 15 -> hd
ce OR cd -> cf
fq AND fr -> ft
at AND az -> bb
ha OR gz -> hb
fp AND fv -> fx
NOT gb -> gc
ia AND ig -> ii
gl OR gm -> gn
0 -> c
NOT ca -> cb
bn RSHIFT 1 -> cg
c LSHIFT 1 -> t
iw OR ix -> iy
kg OR kf -> kh
dy OR ej -> ek
km AND kn -> kp
NOT fc -> fd
hz RSHIFT 3 -> ib
NOT dq -> dr
NOT fg -> fh
dy RSHIFT 2 -> dz
kk RSHIFT 2 -> kl
1 AND fi -> fj
NOT hr -> hs
jp RSHIFT 1 -> ki
bl OR bm -> bn
1 AND gy -> gz
gr AND gt -> gu
db OR dc -> dd
de OR dk -> dl
as RSHIFT 5 -> av
lf RSHIFT 5 -> li
hm AND ho -> hp
cg OR ch -> ci
gj AND gu -> gw
ge LSHIFT 15 -> gi
e OR f -> g
fp OR fv -> fw
fb AND fd -> fe
cd LSHIFT 15 -> ch
b RSHIFT 1 -> v
at OR az -> ba
bn RSHIFT 2 -> bo
lh AND li -> lk
dl AND dn -> do
eg AND ei -> ej
ex AND ez -> fa
NOT kp -> kq
NOT lk -> ll
x AND ai -> ak
jp OR ka -> kb
NOT jd -> je
iy AND ja -> jb
jp RSHIFT 3 -> jr
fo OR fz -> ga
df OR dg -> dh
gj RSHIFT 2 -> gk
gj OR gu -> gv
NOT jh -> ji
ap LSHIFT 1 -> bj
NOT ls -> lt
ir LSHIFT 1 -> jl
bn AND by -> ca
lv LSHIFT 15 -> lz
ba AND bc -> bd
cy LSHIFT 15 -> dc
ln AND lp -> lq
x RSHIFT 1 -> aq
gk OR gq -> gr
NOT kx -> ky
jg AND ji -> jj
bn OR by -> bz
fl LSHIFT 1 -> gf
bp OR bq -> br
he OR hp -> hq
et RSHIFT 5 -> ew
iu RSHIFT 2 -> iv
gl AND gm -> go
x OR ai -> aj
hc OR hd -> he
lg AND lm -> lo
lh OR li -> lj
da LSHIFT 1 -> du
fo RSHIFT 2 -> fp
gk AND gq -> gs
bj OR bi -> bk
lf OR lq -> lr
cj AND cp -> cr
hu LSHIFT 15 -> hy
1 AND bh -> bi
fo RSHIFT 3 -> fq
NOT lo -> lp
hw LSHIFT 1 -> iq
dd RSHIFT 1 -> dw
dt LSHIFT 15 -> dx
dy AND ej -> el
an LSHIFT 15 -> ar
aq OR ar -> as
1 AND r -> s
fw AND fy -> fz
NOT im -> in
et RSHIFT 3 -> ev
1 AND ds -> dt
ec AND ee -> ef
NOT ak -> al
jl OR jk -> jm
1 AND en -> eo
lb OR la -> lc
iu AND jf -> jh
iu RSHIFT 5 -> ix
bo AND bu -> bw
cz OR cy -> da
iv AND jb -> jd
iw AND ix -> iz
lf RSHIFT 1 -> ly
iu OR jf -> jg
NOT dm -> dn
lw OR lv -> lx
gg LSHIFT 1 -> ha
lr AND lt -> lu
fm OR fn -> fo
he RSHIFT 3 -> hg
aj AND al -> am
1 AND kz -> la
dy RSHIFT 5 -> eb
jc AND je -> jf
cm AND co -> cp
gv AND gx -> gy
ev OR ew -> ex
jp AND ka -> kc
fk OR fj -> fl
dy RSHIFT 3 -> ea
NOT bs -> bt
NOT ag -> ah
dz AND ef -> eh
cf LSHIFT 1 -> cz
NOT cv -> cw
1 AND cx -> cy
de AND dk -> dm
ck AND cl -> cn
x RSHIFT 5 -> aa
dv LSHIFT 1 -> ep
he RSHIFT 2 -> hf
NOT bw -> bx
ck OR cl -> cm
bp AND bq -> bs
as OR bd -> be
he AND hp -> hr
ev AND ew -> ey
1 AND lu -> lv
kk RSHIFT 3 -> km
b AND n -> p
NOT kc -> kd
lc LSHIFT 1 -> lw
km OR kn -> ko
id AND if -> ig
ih AND ij -> ik
jr AND js -> ju
ci RSHIFT 5 -> cl
hz RSHIFT 1 -> is
1 AND ke -> kf
NOT gs -> gt
aw AND ay -> az
x RSHIFT 2 -> y
ab AND ad -> ae
ff AND fh -> fi
ci AND ct -> cv
eq LSHIFT 1 -> fk
gj RSHIFT 3 -> gl
u LSHIFT 1 -> ao
NOT bb -> bc
NOT hj -> hk
kw AND ky -> kz
as AND bd -> bf
dw OR dx -> dy
br AND bt -> bu
kk AND kv -> kx
ep OR eo -> eq
he RSHIFT 1 -> hx
ki OR kj -> kk
NOT ju -> jv
ek AND em -> en
kk RSHIFT 5 -> kn
NOT eh -> ei
hx OR hy -> hz
ea OR eb -> ec
s LSHIFT 15 -> w
fo RSHIFT 1 -> gh
kk OR kv -> kw
bn RSHIFT 5 -> bq
NOT ed -> ee
1 AND ht -> hu
cu AND cw -> cx
b RSHIFT 5 -> f
kl AND kr -> kt
iq OR ip -> ir
ci RSHIFT 2 -> cj
cj OR cp -> cq
o AND q -> r
dd RSHIFT 5 -> dg
b RSHIFT 2 -> d
ks AND ku -> kv
b RSHIFT 3 -> e
d OR j -> k
NOT p -> q
NOT cr -> cs
du OR dt -> dv
kf LSHIFT 15 -> kj
NOT ac -> ad
fo RSHIFT 5 -> fr
hz OR ik -> il
jx AND jz -> ka
gh OR gi -> gj
kk RSHIFT 1 -> ld
hz RSHIFT 5 -> ic
as RSHIFT 2 -> at
NOT jy -> jz
1 AND am -> an
ci OR ct -> cu
hg AND hh -> hj
jq OR jw -> jx
v OR w -> x
la LSHIFT 15 -> le
dh AND dj -> dk
dp AND dr -> ds
jq AND jw -> jy
au OR av -> aw
NOT bf -> bg
z OR aa -> ab
ga AND gc -> gd
hz AND ik -> im
jt AND jv -> jw
z AND aa -> ac
jr OR js -> jt
hb LSHIFT 1 -> hv
hf OR hl -> hm
ib OR ic -> id
fq OR fr -> fs
cq AND cs -> ct
ia OR ig -> ih
dd OR do -> dp
d AND j -> l
ib AND ic -> ie
as RSHIFT 3 -> au
be AND bg -> bh
dd AND do -> dq
NOT l -> m
1 AND gd -> ge
y AND ae -> ag
fo AND fz -> gb
NOT ie -> if
e AND f -> h
x RSHIFT 3 -> z
y OR ae -> af
hf AND hl -> hn
NOT h -> i
NOT hn -> ho
he RSHIFT 5 -> hh";
    }
}
