using AutoFixture.Kernel;
using NuGet.Versioning;

namespace ChocolateStoreCoreTests
{
    public class ChocolateStoreSpecimenBuilder : ISpecimenBuilder
    {
        private readonly Random rnd = new Random();

        public object Create(object request, ISpecimenContext context)
        {
            var t = request as Type;
            if (typeof(NuGetVersion).Equals(t))
                return new NuGetVersion(rnd.Next(0, 9), rnd.Next(0, 9), rnd.Next(0, 9));

            return new NoSpecimen();
        }
    }
}
