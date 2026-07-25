namespace Acuaria.Aquarium
{
    public static class AquariumUIText
    {
        public const string Details = "Detalles";
        public const string Close = "Cerrar";
        public const string Temperature = "Temperatura";
        public const string RecommendedRange = "Rango recomendado";
        public const string Inhabitants = "Habitantes";
        public const string Capacity = "Capacidad provisional";
        public const string EducationalHeading = "Consejo educativo";
        public const string NoInhabitants = "Sin habitantes";
        public const string VolumeExplanation =
            "El volumen influye en la estabilidad del agua y en cuántos peces puede albergar el acuario.";

        public static string StatusLabel(AquariumStatus status) => status switch
        {
            AquariumStatus.Excellent => "Excelente",
            AquariumStatus.Good => "Estable",
            AquariumStatus.Attention => "Revisar",
            AquariumStatus.Critical => "Atención urgente",
            _ => "Sin evaluar"
        };
    }
}
