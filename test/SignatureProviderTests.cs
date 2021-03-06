﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace System.IdentityModel.Test
{

    /// <summary>
    /// This class tests:
    /// SignatureProviderFactory
    /// SignatureProvider
    /// SymmetricSignatureProvider
    /// AsymmetricSignatureProvider
    /// </summary>
    [TestClass]
    public class SignatureProviderTests
    {
        /// <summary>
        /// Test Context Wrapper instance on top of TestContext. Provides better accessor functions
        /// </summary>
        protected TestContextProvider _testContextProvider;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassSetup( TestContext testContext )
        { }

        [ClassCleanup]
        public static void ClassCleanup()
        { }

        [TestInitialize]
        public void Initialize()
        {
#if _Verbose
            _verbose = true;
#endif

            _testContextProvider = new TestContextProvider( TestContext );
        }

        [TestMethod]
        [TestProperty( "TestCaseID", "4A9C4A2E-C50F-4A57-A85B-2D6D4F14ADF1" )]
        [Description( "Tests for SignatureProviderFactory" )]
        public void SignatureProviderFactoryTests()
        {
            SignatureProviderFactory factory = new SignatureProviderFactory();

            // Asymmetric / Symmetric both need signature alg specified
            FactoryCreateFor( "Siging:    - algorithm string.Empty", KeyingMaterial.AsymmetricKey_1024, string.Empty, factory, ExpectedException.ArgEx() );
            FactoryCreateFor( "Verifying: - algorithm string.Empty", KeyingMaterial.AsymmetricKey_1024, string.Empty, factory, ExpectedException.ArgEx() );

            // Keytype not supported
            FactoryCreateFor( "Siging:    - SecurityKey type not Asymmetric or Symmetric", NotAsymmetricOrSymmetricSecurityKey.New, SecurityAlgorithms.HmacSha256Signature, factory, ExpectedException.ArgEx( "Jwt10500" ) );
            FactoryCreateFor( "Verifying: - SecurityKey type not Asymmetric or Symmetric", NotAsymmetricOrSymmetricSecurityKey.New, SecurityAlgorithms.RsaSha256Signature, factory, ExpectedException.ArgEx( "Jwt10500" ) );

            // Private keys missing
            FactoryCreateFor( "Siging:    - SecurityKey without private key", KeyingMaterial.AsymmetricKey_Public_2048, SecurityAlgorithms.RsaSha256Signature, factory, ExpectedException.InvalidOp() );
            FactoryCreateFor( "Verifying: - SecurityKey without private key", KeyingMaterial.AsymmetricKey_Public_2048, SecurityAlgorithms.RsaSha256Signature, factory, ExpectedException.Null );

            // Key size checks
            FactoryCreateFor( "Siging:    - AsymmetricKeySize Key to small", KeyingMaterial.AsymmetricKey_1024, SecurityAlgorithms.RsaSha256Signature, factory, ExpectedException.ArgRange( "Jwt10530" ) );

            SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForVerifying = 2048;
            FactoryCreateFor( "Verifying: - AsymmetricKeySize Key to small", KeyingMaterial.AsymmetricKey_1024, SecurityAlgorithms.RsaSha256Signature, factory, ExpectedException.ArgRange( "Jwt10531" ) );
            SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForVerifying = SignatureProviderFactory.AbsoluteMinimumAsymmetricKeySizeInBitsForVerifying;

            SignatureProviderFactory.MinimumSymmetricKeySizeInBits = 512;
            FactoryCreateFor( "Siging:    - SymmetricKeySize Key to small", KeyingMaterial.SymmetricSecurityKey_256, SecurityAlgorithms.HmacSha256Signature, factory, ExpectedException.ArgRange( "Jwt10503" ) );
            FactoryCreateFor( "Verifying: - SymmetricKeySize Key to small", KeyingMaterial.SymmetricSecurityKey_256, SecurityAlgorithms.HmacSha256Signature, factory, ExpectedException.ArgRange( "Jwt10503" ) );
            SignatureProviderFactory.MinimumSymmetricKeySizeInBits = SignatureProviderFactory.AbsoluteMinimumSymmetricKeySizeInBits;
            // setting keys too small
            try
            {
                Console.WriteLine( string.Format( "Testcase: '{0}'", "SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForSigning < AbsoluteMinimumAsymmetricKeySizeInBitsForSigning" ) );
                SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForSigning = SignatureProviderFactory.AbsoluteMinimumAsymmetricKeySizeInBitsForSigning - 10;
                Assert.Fail( string.Format( "Expected exception: '{0}'", typeof( ArgumentOutOfRangeException ) ) );
                SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForSigning = SignatureProviderFactory.AbsoluteMinimumAsymmetricKeySizeInBitsForSigning;
            }
            catch ( Exception ex )
            {
                ExpectedException.ProcessException( ExpectedException.ArgRange( "Jwt10513" ), ex );
            }

            try
            {
                Console.WriteLine( string.Format( "Testcase: '{0}'", "SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForVerifying < AbsoluteMinimumAsymmetricKeySizeInBitsForVerifying" ) );
                SignatureProviderFactory.MinimumAsymmetricKeySizeInBitsForVerifying = SignatureProviderFactory.AbsoluteMinimumAsymmetricKeySizeInBitsForVerifying - 10;
                Assert.Fail( string.Format( "Expected exception: '{0}'", typeof( ArgumentOutOfRangeException ) ) );
            }
            catch ( Exception ex )
            {
                ExpectedException.ProcessException( ExpectedException.ArgRange( "Jwt10527" ), ex );
            }

            try
            {
                Console.WriteLine( string.Format( "Testcase: '{0}'", "SignatureProviderFactory.MinimumSymmetricKeySizeInBits < AbsoluteMinimumSymmetricKeySizeInBits" ) );
                SignatureProviderFactory.MinimumSymmetricKeySizeInBits = SignatureProviderFactory.AbsoluteMinimumSymmetricKeySizeInBits - 10;
                Assert.Fail( string.Format( "Expected exception: '{0}'", typeof( ArgumentOutOfRangeException ) ) );
            }
            catch ( Exception ex )
            {
                ExpectedException.ProcessException( ExpectedException.ArgRange( "Jwt10528" ), ex );
            }

        }

        private void FactoryCreateFor( string testcase, SecurityKey key, string algorithm, SignatureProviderFactory factory, ExpectedException exceptionExpected )
        {
            Console.WriteLine( string.Format( "Testcase: '{0}'", testcase ) );

            try
            {
                if ( testcase.StartsWith( "Siging" ) )
                {
                    factory.CreateForSigning( key, algorithm );
                }
                else
                {
                    factory.CreateForVerifying( key, algorithm );
                }

                Assert.IsFalse( exceptionExpected.Thrown != null , string.Format( "Expected exception: '{0}'", exceptionExpected.Thrown ) );
            }
            catch ( Exception ex )
            {
                ExpectedException.ProcessException( exceptionExpected, ex );
            }
        }

        [TestMethod]
        [TestProperty( "TestCaseID", "F7B5A336-BF04-4589-9F8E-36451E1E3B7F" )]
        [Description( "AsymmetricSignatureProvider Constructor" )]
        public void AsymmetricSignatureProvider_ConstructorTests()
        {
            AsymmetricSecurityKey privateKey = KeyingMaterial.X509SigningCreds_2048_RsaSha2_Sha2.SigningKey as AsymmetricSecurityKey;
            AsymmetricSecurityKey publicKey = KeyingMaterial.X509SigningCreds_Public_2048_RsaSha2_Sha2.SigningKey as AsymmetricSecurityKey;
            string sha2SignatureAlgorithm = KeyingMaterial.X509SigningCreds_2048_RsaSha2_Sha2.SignatureAlgorithm;

            // no errors
            AsymmetricConstructor_Test( "Signing:   - Creates with no errors", privateKey, sha2SignatureAlgorithm );
            AsymmetricConstructor_Test( "Verifying: - Creates with no errors (Private Key)", privateKey, sha2SignatureAlgorithm );
            AsymmetricConstructor_Test( "Verifying: - Creates with no errors (Public Key)", publicKey, sha2SignatureAlgorithm );

            // null, empty algorithm digest
            AsymmetricConstructor_Test( "Signing:   - NUll key", null, sha2SignatureAlgorithm, ExpectedException.ArgNull );
            AsymmetricConstructor_Test( "Signing:   - SignatureAlorithm == null", privateKey, null, ExpectedException.ArgNull );
            AsymmetricConstructor_Test( "Signing:   - SignatureAlorithm == whitespace", privateKey, "    ", ExpectedException.ArgEx( "WIF10002" ) );

            // Private keys missing
            AsymmetricConstructor_Test( "Signing:   - SecurityKey without private key", publicKey, sha2SignatureAlgorithm, ExpectedException.InvalidOp() );
            AsymmetricConstructor_Test( "Verifying: - SecurityKey without private key", publicKey, sha2SignatureAlgorithm, null );

            // _formatter not created
            AsymmetricConstructor_Test( "Signing:   - key cannot create _formatter", KeyingMaterial.AsymmetricKey_2048, "SecurityAlgorithms.RsaSha256Signature", ExpectedException.InvalidOp( "Jwt10518" ) );

            // _deformatter not created
            AsymmetricConstructor_Test( "Verifying: - key cannot create _deformatter", KeyingMaterial.AsymmetricKey_Public_2048, "SecurityAlgorithms.RsaSha256Signature", ExpectedException.InvalidOp( "Jwt10518" ) );

            Console.WriteLine( "Test missing: key.GetHashAlgorithmForSignature( signingCredentials.SignatureAlgorithm );" ); //TODO: Should this be fixed?
        }

        private void AsymmetricConstructor_Test( string testcase, AsymmetricSecurityKey key, string algorithm, ExpectedException exceptionExpected = null )
        {
            Console.WriteLine( string.Format( "Testcase: '{0}'", testcase ) );

            AsymmetricSignatureProvider provider = null;
            try
            {
                if ( testcase.StartsWith( "Signing" ) )
                {
                    provider = new AsymmetricSignatureProvider( key, algorithm, true );
                }
                else
                {
                    provider = new AsymmetricSignatureProvider( key, algorithm, false );
                }

                if (exceptionExpected != null && exceptionExpected.Thrown != null)
                {
                    Assert.Fail("Expected exception: '{0}'", exceptionExpected.Thrown);
                }
            }
            catch ( Exception ex )
            {
                ExpectedException.ProcessException( exceptionExpected, ex );
            }
        }

        [TestMethod]
        [TestProperty( "TestCaseID", "8A43293F-196C-47B8-8C1D-59CDAD30C39E" )]
        [Description( "Tests for AsymmetricSignatureProvider.Dispose" )]
        public void AsymmetricSignatureProvider_Dispose()
        {
            AsymmetricSignatureProvider provider = new AsymmetricSignatureProvider( KeyingMaterial.AsymmetricKey_Public_2048, SecurityAlgorithms.RsaSha256Signature );
            provider.Dispose();

            try
            {
                provider.Sign( new byte[256] );
            }
            catch ( Exception ex )
            {
                ExpectedException.ProcessException( ExpectedException.ObjDisp, ex );
            }

            try
            {
                provider.Verify( new byte[256], new byte[256] );
            }
            catch ( Exception ex )
            {
                ExpectedException.ProcessException( ExpectedException.ObjDisp, ex );
            }

            try
            {
                provider.Dispose();
            }
            catch ( Exception ex )
            {
                Assert.Fail( string.Format( "AsymmetricSignatureProvider.Dispose called twice, caught exception: '{0}'", ex ) );
            }
        }

        [TestMethod]
        [TestProperty( "TestCaseID", "4923DA59-3F32-4995-84D3-C49B0A08EEDE" )]
        [Description( "Tests for Asymmetric and Symmetric SignAndVerify" )]
        public void SignatureProviders_SignAndVerify()
        {
            // asymmetric
            try
            {
                Random r = new Random();
                AsymmetricSignatureProvider provider = new AsymmetricSignatureProvider( KeyingMaterial.AsymmetricKey_2048, SecurityAlgorithms.RsaSha256Signature );
                byte[] bytesin = new byte[1024];
                r.NextBytes( bytesin );
                byte[] signature = provider.Sign( bytesin );
            }
            catch ( Exception ex )
            {
                Assert.IsFalse( ex.GetType() != typeof( InvalidOperationException ) , "ex.GetType() != typeof( InvalidOperationException )" );
            }

            // asymmetric
            try
            {
                Random r = new Random();
                AsymmetricSignatureProvider provider = new AsymmetricSignatureProvider( KeyingMaterial.AsymmetricKey_2048, SecurityAlgorithms.RsaSha256Signature, true );
                byte[] bytesin = new byte[1024];
                r.NextBytes( bytesin );
                byte[] signature = provider.Sign( bytesin );
                Assert.IsFalse( !provider.Verify( bytesin, signature ) , string.Format( "AsymmetricSignatureProvider did not verify" ) );
            }
            catch ( Exception )
            {
                Assert.Fail( "Should have thrown, it is possible that crypto config mapped this." );
            }

            // unknown algorithm
            try
            {
                Random r = new Random();
                AsymmetricSignatureProvider provider = new AsymmetricSignatureProvider( KeyingMaterial.AsymmetricKey_2048, "SecurityAlgorithms.RsaSha256Signature" );
                Assert.Fail( string.Format( "Should have thrown, it is possible that crypto config mapped this." ) );
            }
            catch ( Exception ex )
            {
                Assert.IsFalse( ex.GetType() != typeof( InvalidOperationException ) , "ex.GetType() != typeof( InvalidOperationException )" );
            }

            // symmetric
            try
            {
                Random r = new Random();
                SymmetricSignatureProvider provider = new SymmetricSignatureProvider( KeyingMaterial.SymmetricSecurityKey_256, SecurityAlgorithms.HmacSha256Signature );
                byte[] bytesin = new byte[1024];
                r.NextBytes( bytesin );
                byte[] signature = provider.Sign( bytesin );
                Assert.IsFalse( !provider.Verify( bytesin, signature ) , string.Format( "Signature did not verify" ) );
            }
            catch ( Exception ex )
            {
                Assert.Fail( string.Format( "Unexpected exception received: '{0}'", ex ) );
            }

            // unknown algorithm
            try
            {
                Random r = new Random();
                SymmetricSignatureProvider provider = new SymmetricSignatureProvider( KeyingMaterial.SymmetricSecurityKey_256, "SecurityAlgorithms.HmacSha256Signature" );
                Assert.Fail( string.Format( "Should have thrown, it is possible that crypto config mapped this." ) );
            }
            catch ( Exception ex )
            {
                Assert.IsFalse( ex.GetType() != typeof( InvalidOperationException ) , "ex.GetType() != typeof( InvalidOperationException )" );
            }
        }

        [TestMethod]
        [TestProperty( "TestCaseID", "89AF7B31-7707-4E60-AC32-363C9CA78363" )]
        [Description( "Parameter checking for AsymmetricSignatureProvider..Sign and .Verify" )]
        public void AsymmetricSignatureProvider_Sign_Verify_ParameterChecking()
        {
            AsymmetricSignatureProvider provider = new AsymmetricSignatureProvider( KeyingMaterial.X509SigningCreds_2048_RsaSha2_Sha2.SigningKey as AsymmetricSecurityKey, KeyingMaterial.X509SigningCreds_2048_RsaSha2_Sha2.SignatureAlgorithm );
            Provider_Sign_Verify_ParameterChecking( "Sign - null 'input'", provider, null, null, ExpectedException.ArgNull );
            Provider_Sign_Verify_ParameterChecking( "Sign - zero bytes 'input'", provider, new byte[0], null, ExpectedException.ArgEx( "Jwt10524" ) );
            Provider_Sign_Verify_ParameterChecking( "Sign - _formatter will be null since provider wasn't created with willBeUsedForSigning == true", provider, new byte[1], null, ExpectedException.InvalidOp( "Jwt10520" ) );

            Provider_Sign_Verify_ParameterChecking( "Verify - null 'input'", provider, null, null, ExpectedException.ArgNull );
            Provider_Sign_Verify_ParameterChecking( "Verify - null 'signature'", provider, new byte[1], null, ExpectedException.ArgNull );
            Provider_Sign_Verify_ParameterChecking( "Verify - 'input' zero bytes", provider, new byte[0], new byte[1], ExpectedException.ArgEx( "Jwt10525" ) );
            Provider_Sign_Verify_ParameterChecking( "Verify - 'signature' zero bytes", provider, new byte[1], new byte[0], ExpectedException.ArgEx( "Jwt10526" ) );
        }

        private void Provider_Sign_Verify_ParameterChecking( string testcase, SignatureProvider provider, byte[] bytes, byte[] signature, ExpectedException exceptionExpected = null )
        {
            Console.WriteLine( string.Format( "Testcase: '{0}'", testcase ) );
            try
            {
                if ( testcase.StartsWith( "Sign" ) )
                {
                    provider.Sign( bytes );
                }
                else
                {
                    provider.Verify( bytes, signature );
                }

                Assert.IsFalse( exceptionExpected != null && exceptionExpected.Thrown != null , string.Format( "Expected exception: '{0}'", exceptionExpected.Thrown ) );
            }
            catch ( Exception ex )
            {
                ExpectedException.ProcessException( exceptionExpected, ex );
            }
        }

        [TestMethod]
        [TestProperty( "TestCaseID", "F59BC1A3-C2D7-43F6-99FC-D25E57D1B99C" )]
        [Description( "Tests for SymmetricSignatureProvider Constructor" )]
        public void SymmetricSignatureProviderTests_Constructor()
        {
            // no errors
            SymmetricSignatureProviderTests_Constructor( "Creates with no errors", KeyingMaterial.SymmetricSecurityKey_256, SecurityAlgorithms.HmacSha256Signature );

            // null, empty algorithm digest
            SymmetricSignatureProviderTests_Constructor( "Constructor:   - NUll key", null, SecurityAlgorithms.HmacSha256Signature, ExpectedException.ArgNull );
            SymmetricSignatureProviderTests_Constructor( "Constructor:   - algorithm == string.Empty", KeyingMaterial.SymmetricSecurityKey_256, string.Empty, ExpectedException.ArgEx() );

            // GetKeyedHashAlgorithm throws
            Exception innerException = new CryptographicException( "hi from inner" );
            SymmetricSecurityKey key = new FaultingSymmetricSecurityKey( KeyingMaterial.SymmetricSecurityKey_256, innerException );
            SymmetricSignatureProviderTests_Constructor( "Constructor:   - SecurityKey.GetKeyedHashAlgorithm throws", key, SecurityAlgorithms.HmacSha256Signature, ExpectedException.InvalidOp( "Jwt10532", innerException ) );

            // Key returns null KeyedHash
            key = new FaultingSymmetricSecurityKey( KeyingMaterial.SymmetricSecurityKey_256, null );
            SymmetricSignatureProviderTests_Constructor( "Constructor:   - SecurityKey returns null KeyedHashAlgorithm", key, SecurityAlgorithms.HmacSha256Signature, ExpectedException.InvalidOp( "Jwt10533" ) );

            //_keyedHash.Key = _key.GetSymmetricKey() is null;            
            KeyedHashAlgorithm keyedHashAlgorithm = KeyingMaterial.SymmetricSecurityKey_256.GetKeyedHashAlgorithm( SecurityAlgorithms.HmacSha256Signature );
            key = new FaultingSymmetricSecurityKey( KeyingMaterial.SymmetricSecurityKey_256, null, null, keyedHashAlgorithm, null );
            SymmetricSignatureProviderTests_Constructor( "Constructor:   - key returns null bytes to pass to _keyedHashKey", key, SecurityAlgorithms.HmacSha256Signature, ExpectedException.InvalidOp( "Jwt10534", new NullReferenceException() ) );

        }

        private void SymmetricSignatureProviderTests_Constructor( string testcase, SymmetricSecurityKey key, string algorithm, ExpectedException exceptionExpected = null )
        {
            Console.WriteLine( string.Format( "Testcase: '{0}'", testcase ) );

            SymmetricSignatureProvider provider = null;
            try
            {
                if ( testcase.StartsWith( "Signing" ) )
                {
                    provider = new SymmetricSignatureProvider( key, algorithm );
                }
                else
                {
                    provider = new SymmetricSignatureProvider( key, algorithm );
                }

                if (exceptionExpected != null && exceptionExpected.Thrown != null)
                {
                    Assert.Fail( "Expected exception: '{0}'", exceptionExpected.Thrown );
                }
            }
            catch ( Exception ex )
            {
                ExpectedException.ProcessException( exceptionExpected, ex );
            }
        }

        [TestMethod]
        [TestProperty( "TestCaseID", "E4E5F329-12D8-431A-A971-21F86299DBB1" )]
        [Description( "Parameter checking for SymmetricSignatureProvider.Sign and .Verify" )]
        public void SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking()
        {
            SymmetricSignatureProvider provider = new SymmetricSignatureProvider( KeyingMaterial.SymmetricSigningCreds_256_Sha2.SigningKey as SymmetricSecurityKey, KeyingMaterial.SymmetricSigningCreds_256_Sha2.SignatureAlgorithm );

            SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking( "Sign - null input", provider, null, null, ExpectedException.ArgNull );
            SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking( "Sign - 0 bytes", provider, new byte[0], null, ExpectedException.ArgEx( "Jwt10524" ) );
            SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking( "Sign - 1 byte", provider, new byte[1], null, ExpectedException.Null );

            SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking( "Verify - null input", provider, null, null, ExpectedException.ArgNull );
            SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking( "Verify - null signature", provider, new byte[0], null, ExpectedException.ArgNull );
            SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking( "Verify - 0 bytes input", provider, new byte[0], new byte[0], ExpectedException.ArgEx( "Jwt10525" ) );
            SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking( "Verify - 0 bytes signature", provider, new byte[1], new byte[0], ExpectedException.ArgEx( "Jwt10526" ) );
            SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking( "Verify - 1 byte", provider, new byte[1], new byte[1], ExpectedException.Null );

            provider.Dispose();
            SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking( "Sign - dispose", provider, new byte[1], new byte[1], ExpectedException.ObjDisp );
            SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking( "Verify - dispose", provider, new byte[1], new byte[1], ExpectedException.ObjDisp );
        }

        private void SymmetricSignatureProviderTests_Sign_Verify_ParameterChecking( string testcase, SymmetricSignatureProvider provider, byte[] bytes, byte[] signature, ExpectedException exceptionExpected )
        {
            Console.WriteLine( string.Format( "Testcase: '{0}'", testcase ) );
            try
            {
                if ( testcase.StartsWith( "Sign" ) )
                {
                    provider.Sign( bytes );
                }
                else
                {
                    provider.Verify( bytes, signature );
                }

                Assert.IsFalse( exceptionExpected.Thrown != null , string.Format( "Expected exception: '{0}'", exceptionExpected.Thrown ) );
            }
            catch ( Exception ex )
            {
                ExpectedException.ProcessException( exceptionExpected, ex );
            }
        }
    }
}
