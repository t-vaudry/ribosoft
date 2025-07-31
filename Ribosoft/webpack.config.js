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
            preset: 'minimal',
            colors: true,
            timings: false,
            version: false,
            hash: false,
            builtAt: false,
            assets: false,
            chunks: false,
            modules: false,
            reasons: false,
            children: false,
            source: false,
            errors: true,
            errorDetails: true,
            warnings: true,
            publicPath: false
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
            'details': './ClientApp/Jobs/details.js',
            'jobs-index': './ClientApp/Jobs/index.js'
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
            filename: isDevBuild ? '[name].js' : '[name].js',
            chunkFilename: isDevBuild ? '[name].chunk.js' : '[name].[contenthash:8].chunk.js',
            publicPath: '/dist/',
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
            // Disable splitChunks when using DLL - vendor libs are already bundled
            splitChunks: false,
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
            // Temporarily disable DLL Reference Plugin for testing
            // ...((() => {
            //     try {
            //         const manifestPath = path.join(__dirname, 'wwwroot/dist/vendor-manifest.json');
            //         const manifest = require(manifestPath);
            //         console.log('Using DLL vendor bundle:', manifest.name);
            //         return [new webpack.DllReferencePlugin({
            //             context: __dirname,
            //             manifest: manifest
            //         })];
            //     } catch (e) {
            //         console.warn('vendor-manifest.json not found. Run "npm run build:vendor" first.');
            //         return [];
            //     }
            // })()),
            ...(isDevBuild ? [
                new webpack.HotModuleReplacementPlugin()
            ] : [
                new MiniCssExtractPlugin({
                    filename: '[name].css',
                    chunkFilename: '[name].css'
                })
            ]),
            new webpack.ProvidePlugin({
                // jQuery comes from vendor DLL bundle
                $: 'jquery',
                jQuery: 'jquery', 
                'window.jQuery': 'jquery',
                'window.$': 'jquery'
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
