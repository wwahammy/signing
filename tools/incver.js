// Updates a version number in a file.
var fs = require('fs')
     
if( process.argv.length < 3 )  {
    console.log("ERROR: filename not given");
    return;
}

var inp = fs.createReadStream(process.argv[2] );

inp.on('data', function(chunk) {  
    var newVer = (''+chunk).replace(/(\d*).(\d*).(\d*).(\d*)/, function(a,b,c,d,e) { return b+'.'+c+'.'+(parseInt(d)+1)+'.'+0;}); 
    inp.close();
        
    var outp = fs.createWriteStream(process.argv[2]);
    outp.end(newVer);
});