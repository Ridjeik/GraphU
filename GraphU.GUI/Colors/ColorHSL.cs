namespace GraphU
{
    internal class ColorHSL
    {
        public double Hue { get; set; }

        public double Saturation { get; set; }

        public double Lightness { get; set; }

        public override string? ToString()
        {
            return $"({Hue}; {Saturation}; {Lightness})";
        }
    }
}
