using SourceApi.Model;

namespace SourceApi.Tests.Actions.LoadpointValidator
{
    public static class LoadpointValidatorTestData
    {
        #region ValidLoadpoints
        public static IEnumerable<TestCaseData> ValidLoadpoints
        {
            get
            {
                yield return new TestCaseData(Loadpoint001_3AC_valid);
                yield return new TestCaseData(Loadpoint002_2AC_valid);
                yield return new TestCaseData(Loadpoint003_1AC_valid);
            }
        }

        public static TargetLoadpoint Loadpoint001_3AC_valid
        {
            get
            {
                return new()
                {
                    Phases = new() {
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 5d,
                                },
                                On = true
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 0d,
                                },
                                On = true
                            }
                        },
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 120d,
                                },
                                On = true
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 125d,
                                },
                                On = true
                            }
                        },
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 240d,
                                },
                                On = true
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 245d,
                                },
                                On = true
                            }
                        }
                    },
                    VoltageNeutralConnected = false,
                    Frequency = new()
                    {
                        Mode = FrequencyMode.SYNTHETIC,
                        Value = 50d
                    }
                };
            }
        }

        public static TargetLoadpoint Loadpoint002_2AC_valid
        {
            get
            {
                return new()
                {
                    Phases = new() {
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 5d,
                                },
                                On = true
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 0d,
                                },
                                On = true
                            }
                        },
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 180d,
                                },
                                On = true
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 185d,
                                },
                                On = true
                            }
                        }
                    },
                    VoltageNeutralConnected = false,
                    Frequency = new()
                    {
                        Mode = FrequencyMode.SYNTHETIC,
                        Value = 50d
                    }
                };
            }
        }

        public static TargetLoadpoint Loadpoint003_1AC_valid
        {
            get
            {
                return new()
                {
                    Phases = new() {
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 5d,
                                },
                                On = true
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 0d,
                                },
                                On = true
                            }
                        }
                    },
                    VoltageNeutralConnected = false,
                    Frequency = new()
                    {
                        Mode = FrequencyMode.SYNTHETIC,
                        Value = 50d
                    }
                };
            }
        }
        #endregion

        #region InvalidLoadpoints
        public static IEnumerable<TestCaseData> InvalidLoadpoints
        {
            get
            {
                yield return new TestCaseData(Loadpoint001_3AC_invalid);
                yield return new TestCaseData(Loadpoint002_2AC_invalid);
                yield return new TestCaseData(Loadpoint003_1AC_invalid);
            }
        }

        public static TargetLoadpoint Loadpoint001_3AC_invalid
        {
            get
            {
                return new()
                {
                    Phases = new() {
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 5d,
                                },
                                On = false
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 5d,
                                },
                                On = false
                            }
                        },
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 120d,
                                },
                                On = true
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 125d,
                                },
                                On = true
                            }
                        },
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 240d,
                                },
                                On = true
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 245d,
                                },
                                On = true
                            }
                        }
                    },
                    VoltageNeutralConnected = false,
                    Frequency = new()
                    {
                        Mode = FrequencyMode.SYNTHETIC,
                        Value = 50d
                    }
                };
            }
        }

        public static TargetLoadpoint Loadpoint002_2AC_invalid
        {
            get
            {
                return new()
                {
                    Phases = new() {
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 0d,
                                },
                                On = false
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 5d,
                                },
                                On = false
                            }
                        },
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 180d,
                                },
                                On = true
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 185d,
                                },
                                On = true
                            }
                        }
                    },
                    VoltageNeutralConnected = false,
                    Frequency = new()
                    {
                        Mode = FrequencyMode.SYNTHETIC,
                        Value = 50d
                    }
                };
            }
        }

        public static TargetLoadpoint Loadpoint003_1AC_invalid
        {
            get
            {
                return new()
                {
                    Phases = new() {
                        new() {
                            Voltage = new() {
                                AcComponent = new () {
                                Rms = 230d,
                                Angle = 1d,
                                },
                                On = true
                            },
                            Current = new() {
                                AcComponent = new () {
                                Rms = 60d,
                                Angle = 5d,
                                },
                                On = true
                            }
                        }
                    },
                    VoltageNeutralConnected = false,
                    Frequency = new()
                    {
                        Mode = FrequencyMode.SYNTHETIC,
                        Value = 50d
                    }
                };
            }
        }
        #endregion
    }
}
