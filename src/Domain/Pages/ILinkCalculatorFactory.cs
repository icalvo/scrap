﻿using Scrap.Common;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public interface ILinkCalculatorFactory : IFactory<Job, ILinkCalculator> {}