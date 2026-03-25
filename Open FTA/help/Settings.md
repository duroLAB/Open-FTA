# Application Settings 

## Display settings

### Technical gates
- **Type:** bool  
- **Default:** false  
- **Description:** Enables or disables technical gates.

### Event pen type
- **Type:** PenSettings  
- **Default:** new PenSettings()  
- **Description:** Settings for the pen used to draw events.

### Default event width
- **Type:** int  
- **Default:** 140  
- **Description:** Default event width in pixels.

### Default event height
- **Type:** int  
- **Default:** 90  
- **Description:** Default event height in pixels.

### Default event horizontal spacing
- **Type:** int  
- **Default:** 20  
- **Description:** Default event horizontal spacing in pixels.

### Default event vertical spacing
- **Type:** int  
- **Default:** 80  
- **Description:** Default event vertical spacing in pixels.

### Displayed Metric
- **Type:** DisplayMetricType  
- **Default:** BIM  
- **Description:** Metric displayed in the tree (e.g., BIM).

### Show event frequency
- **Type:** FrequencyDisplayMode  
- **Default:** alwaysHidden  
- **Description:** Controls how event frequency is displayed: it can be shown only for top events, for all events, or hidden completely.

## Sorting algorithm

### Default sorting algorithm
- **Type:** SortingStrategy  
- **Default:** 1  
- **Description:** ALGOI - safe, fast, but not very effective; ALGOII - best results, slower and less safe; ALGOII-left-aligned behaves like ALGOII but aligns to the left.

## Computation settings

### Base time unit
- **Type:** MainCompTimeUnit  
- **Default:** Year  
- **Description:** Base time unit for computations.

### AND gate with two frequency events
- **Type:** GateFrequencyHandling  
- **Default:** (none)  
- **Description:** Specifies whether an AND gate with two frequency events is allowed, disallowed, or allowed with a warning.

### OR gate with mixed event types
- **Type:** GateFrequencyHandling  
- **Default:** (none)  
- **Description:** Specifies whether an OR gate containing both probability and frequency events is allowed, disallowed, or allowed with a warning.

### Simplified probability calculation
- **Type:** bool  
- **Default:** true  
- **Description:** If enabled: P ≈ f ⋅ t. Otherwise: P(failure) = 1 − e^(−f ⋅ t).

### Simplified OR gate
- **Type:** bool  
- **Default:** true  
- **Description:** If enabled: P(A OR B) ≈ PA + PB. Otherwise: P(A OR B) = PA + PB − PA×PB.

## Auto sorting

### Auto-sort tree after copy-paste
- **Type:** bool  
- **Default:** true  
- **Description:** Automatically sorts the tree after copy-paste operations.

### Auto-sort tree after adding/removing FTA item
- **Type:** bool  
- **Default:** true  
- **Description:** Automatically sorts the tree after adding or removing an FTA item.

### Auto-sort tree after collapse/expand
- **Type:** bool  
- **Default:** true  
- **Description:** Automatically sorts the tree after collapsing or expanding the tree structure.
