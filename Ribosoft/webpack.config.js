const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');
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
            extensions: ['.js', '.ts', '.json'],
            alias: {
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
                                sourceMap: isDevBuild,
                                sassOptions: {
                                    silenceDeprecations: ['legacy-js-api']
                                }
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
            clean: {
                keep: /vendor\.(js|css|map)$|vendor-manifest\.json$|assets\//
            },
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
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': JSON.stringify(isDevBuild ? 'development' : 'production')
            }),
            // Only add DllReferencePlugin if vendor-manifest.json exists
            ...((() => {
                try {
                    const manifestPath = path.join(__dirname, 'wwwroot/dist/vendor-manifest.json');
                    require.resolve(manifestPath);
                    return [new webpack.DllReferencePlugin({
                        context: __dirname,
                        manifest: require(manifestPath)
                    })];
                } catch (e) {
                    console.warn('vendor-manifest.json not found. Run "npm run build:vendor:dev" first.');
                    return [];
                }
            })()),
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
            cacheDirectory: path.resolve(__dirname, '.webpack-cache-main'),
            version: 'v2'
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
