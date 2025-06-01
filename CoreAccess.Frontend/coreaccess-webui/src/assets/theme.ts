import {definePreset} from "@primeuix/themes";
import Aura from "@primeuix/themes/aura";

const CoreAccessPreset = definePreset(Aura, {
                colorScheme: {
                    light: {
                        surface: {
                            0: '#ffffff',
                            50: '{zinc.50}',
                            100: '{zinc.100}',
                            200: '{zinc.200}',
                            300: '{zinc.300}',
                            400: '{zinc.400}',
                            500: '{zinc.500}',
                            600: '{zinc.600}',
                            700: '{zinc.700}',
                            800: '{zinc.800}',
                            900: '{zinc.900}',
                            950: '{zinc.950}'
                        }
                    },
                    dark: {
                        surface: {
                            0: '#ffffff',
                            50: '{slate.50}',
                            100: '{slate.100}',
                            200: '{slate.200}',
                            300: '{slate.300}',
                            400: '{slate.400}',
                            500: '{slate.500}',
                            600: '{slate.600}',
                            700: '{slate.700}',
                            800: '{slate.800}',
                            900: '{slate.900}',
                            950: '{slate.950}'
                        }
                    }
                },
                semantic: {
                    primary: {
                        50: '{indigo.50}',
                        100: '{indigo.100}',
                        200: '{indigo.200}',
                        300: '{indigo.300}',
                        400: '{indigo.400}',
                        500: '{indigo.500}',
                        600: '{indigo.600}',
                        700: '{indigo.700}',
                        800: '{indigo.800}',
                        900: '{indigo.900}',
                        950: '{indigo.950}'
                    },
                    secondary: {
                        50: '{blue.50}',
                        100: '{blue.100}',
                        200: '{blue.200}',
                        300: '{blue.300}',
                        400: '{blue.400}',
                        500: '{blue.500}',
                        600: '{blue.600}',
                        700: '{blue.700}',
                        800: '{blue.800}',
                        900: '{blue.900}',
                        950: '{blue.950}'
                    }
                },
                components: {
                    menubar: {
                        colorScheme: {
                            light: {
                                root: {
                                    background: '{surface.800}',
                                    color: '{text.900}'
                                }
                            },
                            dark: {
                                root: {
                                    background: '{surface.800}',
                                    color: '{surface.0}'
                                }
                            }
                        }
                    },
                    breadcrumb: {
                        colorScheme: {
                            light: {
                                root: {
                                    background: '{surface.0}',
                                }
                            },
                            dark: {
                                root: {
                                    background: '{surface.800}'
                                }
                            }
                        }
                    }
                }
            });

export default CoreAccessPreset;