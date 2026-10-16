# Codebase map

## Modules

### Core (`src/Core/`, namespace `Core`)

| File | Type | Purpose |
|---|---|---|
| `ScanProtocol.cs` | `ScanProtocol` (sealed class) | Holds a CT scan protocol's acquisition parameters (name, tube voltage, tube current-time product, slice thickness, pitch) and exposes `Validate()` which returns all `ValidationProblem` instances without side effects. [cite: src/Core/ScanProtocol.cs] |
| `ValidationProblem.cs` | `ValidationProblem` (sealed record) | Immutable description of one validation problem: `FieldName` and human-readable `Description`. Returned in lists by `ScanProtocol.Validate()`. [cite: src/Core/ValidationProblem.cs] |

#### `ScanProtocol` parameter constraints (enforced by `Validate()`)
| Parameter | Constraint |
|---|---|
| `TubeVoltageKv` | Must be one of 70, 80, 100, 120, 140, 150 kV [cite: src/Core/ScanProtocol.cs] |
| `TubeCurrentTimeMas` | 1 – 1000 mAs (inclusive) [cite: src/Core/ScanProtocol.cs] |
| `SliceThicknessMm` | 0.5 – 10 mm (inclusive) [cite: src/Core/ScanProtocol.cs] |
| `Pitch` | 0.5 – 1.5 (inclusive) [cite: src/Core/ScanProtocol.cs] |

A blank `name` throws `ArgumentException` at construction time (not via `Validate()`). [cite: src/Core/ScanProtocol.cs]

## Connections

### Provides
_(none registered)_

### Uses
_(none registered)_
