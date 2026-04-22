using System;
using System.Collections.Generic;
using System.Text;

namespace SeedWave.Core.Generation
{
    public sealed record GeneratedCover(
        byte[] Content,
        string ContentType,
        string FileName);
}
