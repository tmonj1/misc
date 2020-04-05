# set root CA directory
ROOTDIR=${PWD}/root-ca

rm -fr ${ROOTDIR} 2> /dev/null
mkdir -p ${ROOTDIR}

#
# private CA certificate
#

# setup necessary directories and files
mkdir ${ROOTDIR}/newcerts

rm ${ROOTDIR}/index.txt 2> /dev/null 
touch ${ROOTDIR}/index.txt

echo "00" > ${ROOTDIR}/serial

cat - << __EOT__ > ${ROOTDIR}/v3_ca.txt
basicConstraints = critical, CA:true
keyUsage = critical, cRLSign, keyCertSign
subjectKeyIdentifier=hash
__EOT__

# create a private key (without encryption)
openssl genrsa -out ${ROOTDIR}/root_key.pem 2048

# create a CSR
openssl req -new -key ${ROOTDIR}/root_key.pem -subj "/C=JP/ST=Tokyo/O=MyOrg/OU=dev/CN=My CA/" -config ./openssl.cnf -out ${ROOTDIR}/root.csr

# create a certificate
openssl ca -in ${ROOTDIR}/root.csr -selfsign -batch -keyfile ${ROOTDIR}/root_key.pem -notext -config ./openssl.cnf -days 3650 -extfile ${ROOTDIR}/v3_ca.txt -out ${ROOTDIR}/root_crt.pem

# create certificates in DER and pkcs12 (pfx) format, too.
openssl x509 -inform PEM -outform DER -in ${ROOTDIR}/root_crt.pem -out ${ROOTDIR}/root.der
openssl pkcs12 -export -password "pass:" -inkey ${ROOTDIR}/root_key.pem -in ${ROOTDIR}/root_crt.pem -out ${ROOTDIR}/root.pfx

#
# server certificate
#

SERVERDIR=${PWD}/server
rm -fr ${SERVERDIR} 2> /dev/null
mkdir -p ${SERVERDIR}

# create a private key (without encryption)
openssl genrsa -out ${SERVERDIR}/server_key.pem 2048

# create a CSR
openssl req -new -key ${SERVERDIR}/server_key.pem -subj "/C=JP/ST=Tokyo/O=MyOrg/OU=dev/CN=My Servers/" -config ./openssl.cnf -out ${SERVERDIR}/server.csr

# create a certificate
cat - << __EOT__ > ${SERVERDIR}/v3_servers.txt
[SAN]
basicConstraints = CA:false
keyUsage = critical, digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
authorityKeyIdentifier=keyid,issuer

subjectAltName=@alt_names
basicConstraints=CA:FALSE
[alt_names]
DNS.1=localhost
IP.1=127.0.0.1
IP.2=192.168.11.5
__EOT__

openssl ca -batch -in ${SERVERDIR}/server.csr -config ./openssl.cnf -cert ${ROOTDIR}/root_crt.pem -keyfile ${ROOTDIR}/root_key.pem -extfile ${SERVERDIR}/v3_servers.txt -extensions SAN -out ${SERVERDIR}/server_crt.pem -days 365

exit 0

#
# might be
#

# basicConstraints = CA:true ??

