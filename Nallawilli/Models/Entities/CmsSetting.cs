namespace Nallawilli.Models.Entities
{
    public class CmsSetting : BaseEntity
    {
        public string? SiteName { get; set; }
        public string? SiteDescription { get; set; }
        public string? LogoUrl { get; set; }
        public string? FooterLogoUrl { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public string? CopyrightText { get; set; }
    }
}
