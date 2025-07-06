const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const { VueLoaderPlugin } = require('vue-loader');
const HtmlWebpackPlugin = require('html-webpack-plugin');

const bundleOutputDir = './wwwroot/dist';

module.exports = (env, argv) => {
    const isDevBuild = argv.mode === 'development';

    return {
        mode: isDevBuild ? 'development' : 'production',
        stats: { 
            modules: false,
            children: false,
            chunks: false,
            chunkModules: false,
            colors: true,
            timings: true
        },
        context: __dirname,
        resolve: {
            extensions: ['.js', '.vue', '.ts', '.json'],
            alias: {
                'vue$': 'vue/dist/vue.esm-bundler.js',
                '@': path.resolve(__dirname, 'ClientApp'),
                '~': path.resolve(__dirname, 'node_modules')
            },
            fallback: {
                // Add fallbacks for Node.js modules if needed
                "path": false,
                "fs": false
            }
        },
        entry: {
            'main': './ClientApp/boot.js',
            'request': './ClientApp/Request/request.js',
            'details': './ClientApp/Jobs/details.js'
        },
        module: {
            rules: [
                {
                    test: /\.vue$/,
                    loader: 'vue-loader',
                    include: path.resolve(__dirname, 'ClientApp'),
                    options: {
                        compilerOptions: {
                            isCustomElement: tag => tag.startsWith('fornac-')
                        }
                    }
                },
                {
                    test: /\.vue\.html$/,
                    loader: 'vue-loader',
                    include: path.resolve(__dirname, 'ClientApp')
                },
                {
                    test: /\.css$/,
                    use: [
                        isDevBuild ? 'style-loader' : MiniCssExtractPlugin.loader,
                        {
                            loader: 'css-loader',
                            options: {
                                sourceMap: isDevBuild,
                                importLoaders: 1
                            }
                        }
                    ]
                },
                {
                    test: /\.s[ac]ss$/i,
                    use: [
                        isDevBuild ? 'style-loader' : MiniCssExtractPlugin.loader,
                        {
                            loader: 'css-loader',
                            options: {
                                sourceMap: isDevBuild
                            }
                        },
                        {
                            loader: 'sass-loader',
                            options: {
                                sourceMap: isDevBuild
                            }
                        }
                    ]
                },
                {
                    test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|eot|ttf)$/,
                    type: 'asset',
                    parser: {
                        dataUrlCondition: {
                            maxSize: 25000
                        }
                    },
                    generator: {
                        filename: 'assets/[name].[hash:8][ext]'
                    }
                },
                {
                    test: /\.js$/,
                    exclude: /node_modules/,
                    use: {
                        loader: 'babel-loader',
                        options: {
                            presets: [
                                ['@babel/preset-env', {
                                    targets: {
                                        browsers: ['> 1%', 'last 2 versions', 'not dead']
                                    },
                                    modules: false
                                }]
                            ],
                            cacheDirectory: true
                        }
                    }
                }
            ]
        },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: isDevBuild ? '[name].js' : '[name].[contenthash:8].js',
            chunkFilename: isDevBuild ? '[name].chunk.js' : '[name].[contenthash:8].chunk.js',
            publicPath: '../dist/',
            clean: true,
            assetModuleFilename: 'assets/[name].[hash:8][ext]'
        },
        optimization: {
            minimize: !isDevBuild,
            minimizer: [
                new TerserPlugin({
                    terserOptions: {
                        compress: {
                            drop_console: !isDevBuild,
                            drop_debugger: !isDevBuild
                        },
                        format: {
                            comments: false
                        }
                    },
                    extractComments: false
                })
            ],
            splitChunks: {
                chunks: 'all',
                cacheGroups: {
                    vendor: {
                        test: /[\\/]node_modules[\\/]/,
                        name: 'vendors',
                        chunks: 'all',
                        priority: 10
                    },
                    common: {
                        name: 'common',
                        minChunks: 2,
                        chunks: 'all',
                        priority: 5,
                        reuseExistingChunk: true
                    }
                }
            },
            runtimeChunk: {
                name: 'runtime'
            },
            moduleIds: 'deterministic',
            chunkIds: 'deterministic'
        },
        plugins: [
            new VueLoaderPlugin(),
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': JSON.stringify(isDevBuild ? 'development' : 'production'),
                '__VUE_OPTIONS_API__': JSON.stringify(true),
                '__VUE_PROD_DEVTOOLS__': JSON.stringify(isDevBuild),
                '__VUE_PROD_HYDRATION_MISMATCH_DETAILS__': JSON.stringify(isDevBuild)
            }),
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require('./wwwroot/dist/vendor-manifest.json')
            }),
            ...(isDevBuild ? [
                new webpack.HotModuleReplacementPlugin()
            ] : [
                new MiniCssExtractPlugin({
                    filename: '[name].[contenthash:8].css',
                    chunkFilename: '[name].[contenthash:8].css'
                })
            ]),
            new webpack.ProvidePlugin({
                $: 'jquery',
                jQuery: 'jquery',
                'window.jQuery': 'jquery'
            })
        ],
        devtool: isDevBuild ? 'eval-source-map' : 'source-map',
        cache: {
            type: 'filesystem',
            buildDependencies: {
                config: [__filename]
            },
            cacheDirectory: path.resolve(__dirname, '.webpack-cache')
        },
        performance: {
            hints: isDevBuild ? false : 'warning',
            maxEntrypointSize: 512000,
            maxAssetSize: 512000
        },
        experiments: {
            topLevelAwait: true
        }
    };
};
