const path = require('path')
const webpack = require('webpack')
const ExtractTextPlugin = require('extract-text-webpack-plugin')
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin
const merge = require('webpack-merge')
const WebpackNotifierPlugin = require('webpack-notifier')

module.exports = env => {
  const isDevBuild = !(env && env.prod)

  var extractPlugin = new ExtractTextPlugin({
    filename: 'styles/site.css',
    allChunks: true,
    disable: false
  })

  const sharedConfig = () => ({
    stats: {
      modules: false
    },
    resolve: {
      extensions: ['.js', '.jsx', '.ts', '.tsx']
    },
    output: {
      filename: '[name].js',
      publicPath: 'dist/'
    },
    module: {
      rules: [
        // {
        //   test: /\.jsx?$/,
        //   loader: 'babel-loader',
        //   exclude: /node_modules/
        // },
        {
          test: /\.tsx?$/,
          include: /ClientApp/,
          use: 'awesome-typescript-loader?silent=true'
        },
        {
          test: /\.s?css$/,
          use: ['css-hot-loader'].concat(
            extractPlugin.extract({
              use: [
                {
                  loader: 'css-loader',
                  options: {
                    sourceMap: true
                  }
                },
                {
                  loader: 'resolve-url-loader',
                  options: {
                    sourceMap: true
                  }
                },
                {
                  loader: 'sass-loader',
                  options: {
                    sourceMap: true
                  }
                }
              ]
            })
          )
        },
        {
          test: /\.(woff|woff2|eot|ttf|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
          use: 'url-loader?limit=100000&name=fonts/[name].[ext]'
        },
        {
          test: /\.(png|jpg|jpeg|gif)$/,
          use: 'url-loader?limit=100000&name=images/[name].[ext]'
        }
        //{ test: /\.css$/, use: ExtractTextPlugin.extract({ use: isDevBuild ? 'css-loader' : 'css-loader?minimize' }) }
      ]
    },
    plugins: [new CheckerPlugin()]
  })

  // Configuration for client-side bundle suitable for running in browsers
  const clientBundleOutputDir = './wwwroot/dist'
  const clientBundleConfig = merge(sharedConfig(), {
    entry: {
      main: './ClientApp/main.tsx'
    },
    output: {
      path: path.join(__dirname, clientBundleOutputDir)
    },
    plugins: [
      extractPlugin,
      new WebpackNotifierPlugin(),
      new webpack.ProvidePlugin({
        $: 'jquery',
        jQuery: 'jquery'
      }),
      new webpack.DllReferencePlugin({
        context: __dirname,
        manifest: require('./wwwroot/dist/vendor-manifest.json')
      })
    ].concat(
      isDevBuild
        ? [
            // Plugins that apply in development builds only
            new webpack.SourceMapDevToolPlugin({
              filename: '[file].map', // Remove this line if you prefer inline source maps
              moduleFilenameTemplate: path.relative(clientBundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
            })
          ]
        : [
            // Plugins that apply in production builds only
            // new webpack.optimize.UglifyJsPlugin()
          ]
    )
  })

  return [clientBundleConfig]
}
