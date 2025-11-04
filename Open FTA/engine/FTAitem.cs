public class FTAitem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid GuidCode { get; set; }
    public Guid Parent { get; set; }
    public int ItemType { get; set; } // 1 - Basic Event, 2 - Intermediate Event
    // public int Gate; // voliteľné, ak nepotrebné
    public string Tag { get; set; }
    public double Frequency { get; set; }
    public double Value { get; set; }
    // public int UserMetricType; // voliteľné
    public int ValueUnit { get; set; }
    public ValueTypes ValueType { get; set; }
    public Gates Gate { get; set; }

    public List<Guid> Children { get; set; }
    public int Level { get; set; }
    public double X1 { get; set ; }
    public double Y1 { get; set; }
 

    private int _x;
    private int _y;

    public int X
    {
        get => _x;
        set
        {
            _x = value;
       
            // rect = new RectangleF(_x, rect.Y, rect.Width, rect.Height);
             rect = new RectangleF(_x - Constants.EventHorizontalSpacing / 2, rect.Y, Constants.EventWidth + Constants.EventHorizontalSpacing, rect.Height);
        }
    }

    public int Y
    {
        get => _y;
        set
        {
            _y = value;            
           // rect = new RectangleF(rect.X, _y, Constants.EventWidth , rect.Height);
            rect = new RectangleF(_x - Constants.EventHorizontalSpacing / 2, _y, Constants.EventWidth + Constants.EventHorizontalSpacing, rect.Height);
        }
    }


    public int level { get; set; }
    public bool ItemState { get; set; }
    public bool IsHidden { get; set; }
    public bool IsSelected { get; set; }
    public double LowerBoundFrequency { get; set; }
    public double UpperBoundFrequency { get; set; }
    public double BIM { get; set; }

    public string Reference{ get; set; }

    public RectangleF rect { get; set; }

    public FTAitem()
    {
        // generovanie unikátneho Guid
        GuidCode = Guid.NewGuid();
        Children = new List<Guid>();
        LowerBoundFrequency = 0;
        UpperBoundFrequency = 0;
        BIM = 0;

        rect = new Rectangle(0,0,Constants.EventWidth,Constants.EventHeight);

    }

    public FTAitem DeepCopyFrom(FTAitem source)
    {
        if (source == null)
            return null;

        FTAitem copy = new FTAitem
        {
            Name = source.Name,
            Description = source.Description,
            Parent = source.Parent,
            ItemType = source.ItemType,
            Tag = source.Tag,              
            Frequency = source.Frequency,
            Value = source.Value,
            ValueUnit = source.ValueUnit,
            ValueType = source.ValueType,
            Gate = source.Gate,
            Level = source.Level,
            X1 = source.X1,
            Y1 = source.Y1,
            X = source.X,
            Y = source.Y,
            ItemState = source.ItemState,
            IsHidden = source.IsHidden,
            IsSelected = source.IsSelected,
            LowerBoundFrequency = source.LowerBoundFrequency,
            UpperBoundFrequency = source.UpperBoundFrequency,
            BIM = source.BIM,
            Reference = source.Reference,

            // nový GUID pre kópiu
            GuidCode = Guid.NewGuid(),

            // deep copy listu
            Children = new List<Guid>(source.Children)
        };

        return copy;
    }
}





