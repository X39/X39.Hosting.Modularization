﻿using X39.Hosting.Modularization.Samples.CommonLibrary.Common;

namespace X39.Hosting.Modularization.Samples.CommonLibrary.ModuleC;

public sealed class DependantClass(ISingletonClass singletonClass, IScopedClass scopedClass, ITransientClass transientClass)
{
    
}
