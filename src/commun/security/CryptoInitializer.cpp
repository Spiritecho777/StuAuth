#include "CryptoInitializer.h"

#include <openssl/evp.h>
#include <openssl/rand.h>
#include <openssl/conf.h>

void CryptoInitializer::init() 
{
	OPENSSL_init_crypto(OPENSSL_INIT_LOAD_CONFIG, nullptr);
	RAND_poll();
}