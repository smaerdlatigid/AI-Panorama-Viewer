using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class MarketPlace : MonoBehaviour
{
    // EasyExchange contract ABI - TODO update
    string market_abi = "[{\"anonymous\": false, \"inputs\": [{\"indexed\": true, \"internalType\": \"address\", \"name\": \"nftContract\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}, {\"indexed\": false, \"internalType\": \"uint256\", \"name\": \"price\", \"type\": \"uint256\"}, {\"indexed\": false, \"internalType\": \"string\", \"name\": \"memo\", \"type\": \"string\"}], \"name\": \"ItemListedForSale\", \"type\": \"event\"}, {\"anonymous\": false, \"inputs\": [{\"indexed\": true, \"internalType\": \"address\", \"name\": \"nftContract\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}, {\"indexed\": false, \"internalType\": \"uint256\", \"name\": \"price\", \"type\": \"uint256\"}, {\"indexed\": false, \"internalType\": \"string\", \"name\": \"memo\", \"type\": \"string\"}], \"name\": \"ItemSold\", \"type\": \"event\"}, {\"anonymous\": false, \"inputs\": [{\"indexed\": true, \"internalType\": \"address\", \"name\": \"previousOwner\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"address\", \"name\": \"newOwner\", \"type\": \"address\"}], \"name\": \"OwnershipTransferred\", \"type\": \"event\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"nftContract\", \"type\": \"address\"}, {\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"BuyItem\", \"outputs\": [], \"stateMutability\": \"payable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"nftContract\", \"type\": \"address\"}, {\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"CheckItemPrice\", \"outputs\": [{\"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"nftContract\", \"type\": \"address\"}, {\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}, {\"internalType\": \"uint256\", \"name\": \"price\", \"type\": \"uint256\"}, {\"internalType\": \"string\", \"name\": \"memo\", \"type\": \"string\"}], \"name\": \"ListItemForSale\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"getContractBalance\", \"outputs\": [{\"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"owner\", \"outputs\": [{\"internalType\": \"address\", \"name\": \"\", \"type\": \"address\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"renounceOwnership\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"newOwner\", \"type\": \"address\"}], \"name\": \"transferOwnership\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"withdrawContractBalance\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}]";

    // Create dictionary for chains and addresses
    public Dictionary<string, string> market_address = new Dictionary<string, string>{
        { "mumbai", "0x0A4231fbE5E24e99ECA10570053Bd2098E92F05a" },
        { "polygon", "0xa3a97b5fF135aa5e9dCBDcf4f922dE49B009012F" },
        { "eth", "0x66D63D229DA3fa44D532760F3ED65020d039eA84"}
    };

    static Dictionary<string, string> rpc_endpoints = new Dictionary<string, string>{
        {"mumbai", "https://polygon-mumbai.infura.io/v3/4ea543f958fa4eef8153c58fded67583" },
        {"eth", "https://eth-mainnet.g.alchemy.com/v2/P679YGK_yU_Jesy8HDLAPIRFtpysdth0"},
        {"polygon", "https://polygon-mainnet.g.alchemy.com/v2/hGJZ6UCLm_TxhIblt9bA3RWuUF-9DQJY"},
    };

    static Dictionary<string, string> rpc_endpoints_alt = new Dictionary<string, string>{
        {"eth", "https://mainnet.infura.io/v3/4ea543f958fa4eef8153c58fded67583"},
        {"polygon", "https://polygon-mainnet.infura.io/v3/4ea543f958fa4eef8153c58fded67583"},
    };

    // https://chainlist.org/
    Dictionary<string, string> chaind_ids = new Dictionary<string, string>{
        { "mumbai", "80001" },
        { "polygon", "137" },
        { "eth", "1" }
    };

    public Dictionary<string, string> currency = new Dictionary<string, string>{
        { "mumbai", "matic" },
        { "polygon", "matic"},
        { "eth", "eth"},
        { "ethereum", "eth"}
    };

    // ERC721 contract with custom minting on mumbai
    static string mint_abi = "[{\"inputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"constructor\"}, {\"anonymous\": false, \"inputs\": [{\"indexed\": true, \"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"address\", \"name\": \"approved\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"Approval\", \"type\": \"event\"}, {\"anonymous\": false, \"inputs\": [{\"indexed\": true, \"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"address\", \"name\": \"operator\", \"type\": \"address\"}, {\"indexed\": false, \"internalType\": \"bool\", \"name\": \"approved\", \"type\": \"bool\"}], \"name\": \"ApprovalForAll\", \"type\": \"event\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\"}, {\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"approve\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"string\", \"name\": \"_uri\", \"type\": \"string\"}], \"name\": \"mint\", \"outputs\": [], \"stateMutability\": \"payable\", \"type\": \"function\"}, {\"anonymous\": false, \"inputs\": [{\"indexed\": true, \"internalType\": \"address\", \"name\": \"previousOwner\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"address\", \"name\": \"newOwner\", \"type\": \"address\"}], \"name\": \"OwnershipTransferred\", \"type\": \"event\"}, {\"inputs\": [], \"name\": \"renounceOwnership\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\"}, {\"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\"}, {\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"safeTransferFrom\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\"}, {\"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\"}, {\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}, {\"internalType\": \"bytes\", \"name\": \"_data\", \"type\": \"bytes\"}], \"name\": \"safeTransferFrom\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"uint256\", \"name\": \"price\", \"type\": \"uint256\"}], \"name\": \"set_price\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"operator\", \"type\": \"address\"}, {\"internalType\": \"bool\", \"name\": \"approved\", \"type\": \"bool\"}], \"name\": \"setApprovalForAll\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"anonymous\": false, \"inputs\": [{\"indexed\": true, \"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"Transfer\", \"type\": \"event\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\"}, {\"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\"}, {\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"transferFrom\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"newOwner\", \"type\": \"address\"}], \"name\": \"transferOwnership\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"uint256\", \"name\": \"_tokenId\", \"type\": \"uint256\"}, {\"internalType\": \"string\", \"name\": \"_uri\", \"type\": \"string\"}], \"name\": \"updateMetadataOwner\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"withdrawContractBalance\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"stateMutability\": \"payable\", \"type\": \"receive\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\"}], \"name\": \"balanceOf\", \"outputs\": [{\"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"get_price\", \"outputs\": [{\"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"getApproved\", \"outputs\": [{\"internalType\": \"address\", \"name\": \"\", \"type\": \"address\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"getContractBalance\", \"outputs\": [{\"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\"}, {\"internalType\": \"address\", \"name\": \"operator\", \"type\": \"address\"}], \"name\": \"isApprovedForAll\", \"outputs\": [{\"internalType\": \"bool\", \"name\": \"\", \"type\": \"bool\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"mint_price\", \"outputs\": [{\"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"name\", \"outputs\": [{\"internalType\": \"string\", \"name\": \"\", \"type\": \"string\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"owner\", \"outputs\": [{\"internalType\": \"address\", \"name\": \"\", \"type\": \"address\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"ownerOf\", \"outputs\": [{\"internalType\": \"address\", \"name\": \"\", \"type\": \"address\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"bytes4\", \"name\": \"interfaceId\", \"type\": \"bytes4\"}], \"name\": \"supportsInterface\", \"outputs\": [{\"internalType\": \"bool\", \"name\": \"\", \"type\": \"bool\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"symbol\", \"outputs\": [{\"internalType\": \"string\", \"name\": \"\", \"type\": \"string\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"tokens\", \"outputs\": [{\"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"tokenURI\", \"outputs\": [{\"internalType\": \"string\", \"name\": \"\", \"type\": \"string\"}], \"stateMutability\": \"view\", \"type\": \"function\"}]";

    Dictionary<string, string> mint_address = new Dictionary<string, string>{
        { "mumbai", "0x178cA326f68cCF8cD368659d28084B3FDCf5B7CF" },
    };

    // Start is called before the first frame update
    void Start()
    {
        // query api for random nft, load in media container
    }

    // Update is called once per frame
    void Update()
    {

    }

    string txhash = "fail";
    async public void Buy(nft snft)
    {
        // value in wei
        string value = $"{snft.price}";

        // smart contract method to call
        string method = "BuyItem";

        // check if easy exchange supports NFT
        if (market_address.ContainsKey(snft.chain))
        {
            string args = $"[\"{snft.address}\", \"{snft.token_id}\"]";

            // create data to interact with smart contract
            string data = await EVM.CreateContractData(market_abi, method, args);
            // gas limit OPTIONAL
            string gasLimit = "";
            // gas price OPTIONAL
            string gasPrice = "";
            // send transaction
            txhash = await Web3Wallet.SendTransaction(chaind_ids[snft.chain], market_address[snft.chain], value, data, gasLimit, gasPrice);
            Debug.Log($"txhash: {txhash}");
            UploadTxHash(snft.chain, txhash);
        }
    }
    async public void Buy(nft snft, Action callback)
    {
        // value in wei
        string value = $"{snft.price}";

        // smart contract method to call
        string method = "BuyItem";

        // check if easy exchange supports NFT
        if (market_address.ContainsKey(snft.chain))
        {
            string args = $"[\"{snft.address}\", \"{snft.token_id}\"]";

            // create data to interact with smart contract
            string data = await EVM.CreateContractData(market_abi, method, args);
            // gas limit OPTIONAL
            string gasLimit = "";
            // gas price OPTIONAL
            string gasPrice = "";
            // send transaction
            try
            {
                txhash = await Web3Wallet.SendTransaction(chaind_ids[snft.chain], market_address[snft.chain], value, data, gasLimit, gasPrice);
                Debug.Log($"txhash: {txhash}");
                UploadTxHash(snft.chain, txhash);
                callback();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
    
    async public void checkPrice(nft snft)
    {
        // smart contract method to call
        string method = "CheckItemPrice";

        // args: tokenID
        string args = $"[\"{snft.address}\", {snft.token_id}]";

        string network = "mainnet";
        // if chain in test nets, use testnet
        if (snft.chain == "mumbai" || snft.chain == "ropsten" || snft.chain == "rinkeby")
        {
            network = "testnet";
        }

        try {
            // check if easy exchange supports NFT
            if (market_address.ContainsKey(snft.chain))
            {
                // connects to user's browser wallet to call a transaction
                string response = await EVM.Call(snft.chain, network, market_address[snft.chain], market_abi, method, args, rpc_endpoints[snft.chain]);

                if (!response.Contains("502 Bad Gateway"))
                {
                    snft.price = float.Parse(response)/1000000000000000000;
                }
                else
                {
                    Debug.Log("502 Bad Gateway");
                    snft.price = 0;
                }
            }
        } catch (Exception e) {
            // $"Error: {e.Message}";
            Debug.Log($"error fetching price for nft: {snft.chain}, {snft.address}, {snft.token_id}");
            Debug.LogException(e, this);
        }
    }

    async public void checkPrice(nft snft, Action callback)
    {
        // smart contract method to call
        string method = "CheckItemPrice";

        // args: tokenID
        string args = $"[\"{snft.address}\", {snft.token_id}]";

        string network = "mainnet";
        // if chain in test nets, use testnet
        if (snft.chain == "mumbai" || snft.chain == "ropsten" || snft.chain == "rinkeby")
        {
            network = "testnet";
        }

        try {
            // check if easy exchange supports NFT
            if (market_address.ContainsKey(snft.chain))
            {
                // connects to user's browser wallet to call a transaction
                string response = await EVM.Call(snft.chain, network, market_address[snft.chain], market_abi, method, args, rpc_endpoints[snft.chain]);

                if (!response.Contains("502 Bad Gateway"))
                {
                    snft.price = float.Parse(response); // in wei
                    // multiply by 1000000000000000000 to get price in ether
                    callback();
                }
                else
                {
                    Debug.Log("502 Bad Gateway");
                    snft.price = 0;
                }
            }
        } catch (Exception e) {
            // $"Error: {e.Message}";
            Debug.Log($"error fetching price for nft: {snft.chain}, {snft.address}, {snft.token_id}");
            Debug.LogException(e, this);
        }
    }

    async public void Sell(nft snft, Action callback)
    {
        // value in wei
        string value = "0";

        // smart contract method to call
        string method = "ListItemForSale";

        // check if market supports NFT
        if (market_address.ContainsKey(snft.chain))
        {
            // address, token_id, price, memo
            double price = (double)(snft.price*10000000); // convert price to wei based on chain
            string sprice = price.ToString("F0");
            Debug.Log($"price: {sprice}");
            string[] obj = {snft.address, $"{snft.token_id}", $"{sprice}00000000000", "memo" };
            string args = JsonConvert.SerializeObject(obj);

            // create data to interact with smart contract
            string data = await EVM.CreateContractData(market_abi, method, args);
            // gas limit OPTIONAL
            string gasLimit = "";
            // gas price OPTIONAL
            string gasPrice = "";
            // send transaction
            try
            {
                txhash = await Web3Wallet.SendTransaction(chaind_ids[snft.chain], market_address[snft.chain], value, data, gasLimit, gasPrice);
            
                Debug.Log($"txhash: {txhash}");

                WaitForTx(snft.chain, txhash, callback);
                // send to server
                UploadTxHash(snft.chain, txhash);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }

    async public void Transfer(nft snft, string toAddress, Action callback)
    {
        // value in wei
        string value = "0";

        // smart contract method to call
        string method = "transferFrom";
        string account = PlayerPrefs.GetString("Account");
        // from, to, tokenid
        string args = $"[\"{account}\", \"{toAddress}\", {snft.token_id}]";
        Debug.Log($"args: {args}");
        // create data to interact with smart contract
        string data = await EVM.CreateContractData(mint_abi, method, args);
        // gas limit OPTIONAL
        string gasLimit = "";
        // gas price OPTIONAL
        string gasPrice = "";
        // send transaction
        try
        {
            txhash = await Web3Wallet.SendTransaction(chaind_ids[snft.chain], snft.address, value, data, gasLimit, gasPrice);

            Debug.Log($"txhash: {txhash}");

            WaitForTx(snft.chain, txhash, callback);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    async public void WaitForTx(string chain, string tx, Action successCallback, Action failCallback = null)
    {
        if(chain == "eth")
        {
            chain = "ethereum";
        }
        string txStatus = await EVM.TxStatus(chain, "mainnet", tx);
        Debug.Log($"tx status: {txStatus}"); // success, fail, pending
        // TODO add timeout for tx to be confirmed or fail
        while (txStatus != "success")
        {
            txStatus = await EVM.TxStatus(chain, "mainnet", tx);
            Debug.Log($"tx: {txStatus}"); // success, fail, pending

            if (txStatus == "fail")
            {
                break;
            }
            await Task.Delay(1000);

        }

        if (txStatus == "success")
        {
            GetComponent<NFTWallet>().loggedIn = true;
            successCallback();
        }
        else if (txStatus == "fail")
        {
            failCallback();
        }
    }


    async public void tokenURI(nft snft)
    {
        if (String.IsNullOrWhiteSpace(snft.chain))
        {
            // smart contract method to call
            string method = "tokenURI";

            // args: tokenID
            string args = $"[\"{snft.token_id}\"]";

            string network = "mainnet";
            // if chain in test nets, use testnet
            if (snft.chain == "mumbai" || snft.chain == "ropsten" || snft.chain == "rinkeby")
            {
                network = "testnet";
            }

            try {
                // connects to user's browser wallet to call a transaction
                snft.tokenuri = await EVM.Call(snft.chain, network, snft.address, mint_abi, method, args, rpc_endpoints[snft.chain]);
                ParseTool.DownloadTokenURI(snft);
            } catch (Exception e) {
                //Status.text = $"Error: {e.Message}";
                Debug.LogException(e, this);
            }
        }
        else
        {
            print("chain not set");
        }
    }

    async public void checkOwner(nft snft)
    {
        if (String.IsNullOrWhiteSpace(snft.owner))
        {
            // smart contract method to call
            string method = "ownerOf";

            // args: tokenID
            string args = $"[\"{snft.token_id}\"]";

            string network = "mainnet";
            // if chain in test nets, use testnet
            if (snft.chain == "mumbai" || snft.chain == "ropsten" || snft.chain == "rinkeby")
            {
                network = "testnet";
            }

            try {
                // connects to user's browser wallet to call a transaction
                snft.owner = await EVM.Call(snft.chain, network, snft.address, mint_abi, method, args, rpc_endpoints[snft.chain]);
            } catch (Exception e) {
                //Status.text = $"Error: {e.Message}";
                Debug.LogException(e);
            }
        }
        else
        {
            print("owner already set");
            // todo add override/update?
        }
    }


    async public void checkApproved(nft snft, Action callback)
    {
        // check nft chain in marketplace contracts
        if (market_address.ContainsKey(snft.chain))
        {
            // smart contract method to call
            string method = "getApproved";

            // args: tokenID
            string args = $"[{snft.token_id}]";

            string network = "mainnet";
            // if chain in test nets, use testnet
            if (snft.chain == "mumbai" || snft.chain == "ropsten" || snft.chain == "rinkeby")
            {
                network = "testnet";
            }

            try {
                // connects to user's browser wallet to call a transaction
                string response = await EVM.Call(snft.chain, network, snft.address, mint_abi, method, args, rpc_endpoints[snft.chain]);
                if (response.Contains("bad gateway"))
                {
                    response = await EVM.Call(snft.chain, network, snft.address, mint_abi, method, args, rpc_endpoints_alt[snft.chain]);
                    snft.marketApproved = (response == market_address[snft.chain]);
                    callback();
                }
                else
                {
                    snft.marketApproved = (response == market_address[snft.chain]);
                    callback();
                }
                // returns address
            } catch (Exception e) {
                //Status.text = $"Error: {e.Message}";
                Debug.LogException(e, this);
            }
        }
    }

    async public void checkApproved(nft snft)
    {
        // check nft chain in marketplace contracts
        if (market_address.ContainsKey(snft.chain))
        {
            // smart contract method to call
            string method = "getApproved";

            // args: tokenID
            string args = $"[{snft.token_id}]";

            string network = "mainnet";
            // if chain in test nets, use testnet
            if (snft.chain == "mumbai" || snft.chain == "ropsten" || snft.chain == "rinkeby")
            {
                network = "testnet";
            }

            try {
                // connects to user's browser wallet to call a transaction
                string response = await EVM.Call(snft.chain, network, snft.address, mint_abi, method, args, rpc_endpoints[snft.chain]);
                if (response.Contains("bad gateway"))
                {
                    // try again with alternate rpc endpoint
                    Debug.Log("Could not check approved, try with alternate rpc endpoint");
                }
                else
                {
                    snft.marketApproved = (response == market_address[snft.chain]);
                }
                
                // returns address
            } catch (Exception e) {
                //Status.text = $"Error: {e.Message}";
                Debug.LogException(e, this);
            }
        }
    }

    async public void Approve(nft snft, Action successCallback, Action failureCallback=null)
    {
        // check if market supports NFT
        if (market_address.ContainsKey(snft.chain))
        {
            // value in wei
            string value = "0";
            // smart contract method to call
            string method = "approve";
            // nft info
            string args = $"[\"{market_address[snft.chain]}\", \"{snft.token_id}\"]";
            // create data to interact with smart contract
            string data = await EVM.CreateContractData(mint_abi, method, args);
            // gas limit OPTIONAL
            string gasLimit = "";
            // gas price OPTIONAL
            string gasPrice = "";
            // send transaction
            try{
                txhash = await Web3Wallet.SendTransaction(chaind_ids[snft.chain], snft.address, value, data, gasLimit, gasPrice);
            }
            catch (Exception e)
            {
                //Status.text = $"Error: {e.Message}";
                Debug.LogException(e, this);
                if (failureCallback != null)
                {
                    failureCallback();
                }
            }
            Debug.Log($"txhash: {txhash}");
            WaitForTx(snft.chain, txhash, successCallback, failureCallback);
        }
        // start coroutine to continuously poll until transaction is mined?
    }

    async public void ApproveAll(nft snft)
    {
        // check if market supports NFT
        if (market_address.ContainsKey(snft.chain))
        {
            // value in wei
            string value = "0";
            // smart contract method to call
            string method = "setApprovalForAll";
            // nft info
            string args = $"[\"{market_address[snft.chain]}\", true]";
            // create data to interact with smart contract
            string data = await EVM.CreateContractData(mint_abi, method, args);
            // gas limit OPTIONAL
            string gasLimit = "";
            // gas price OPTIONAL
            string gasPrice = "";
            // send transaction
            txhash = await Web3Wallet.SendTransaction(chaind_ids[snft.chain], snft.address, value, data, gasLimit, gasPrice);
            Debug.Log($"txhash: {txhash}");
        }
        // start coroutine to continuously poll until transaction is mined?
    }


    async public void Burn(nft snft)
    {
                // check if market supports NFT
        // value in wei
        string value = "0";
        // smart contract method to call
        string method = "transferFrom";
        // nft info
        string args = $"[\"{snft.owner}\", \"000000000000000000000000000000000000dEaD\", \"{snft.token_id}\"]";
        // create data to interact with smart contract
        string data = await EVM.CreateContractData(mint_abi, method, args);
        // gas limit OPTIONAL
        string gasLimit = "";
        // gas price OPTIONAL
        string gasPrice = "";
        // send transaction
        txhash = await Web3Wallet.SendTransaction(chaind_ids[snft.chain], snft.address, value, data, gasLimit, gasPrice);
        Debug.Log($"txhash: {txhash}");
        // start coroutine to continuously poll until transaction is mined?
    }

    class TxHash {
        public string txhash;
        public string chain;
        public TxHash (string txhash, string chain)
        {
            this.txhash = txhash;
            this.chain = chain;
        }
    }

    public string api_url = "http://127.0.0.1:8888";

    public async void UploadTxHash(string chain, string txhash)
    {
        // get nft metadata as string
        Debug.Log("Uploading txhash...");
        
        TxHash tx = new TxHash(txhash, chain);
        var itemToSend = JsonUtility.ToJson(tx);
        
        // build web request
        var request = (HttpWebRequest) WebRequest.Create(new Uri($"{api_url}/upload/tx"));
        request.ContentType = "application/json";
        request.Method = "POST";
        request.Timeout = 4000; //ms
        
        // stream request
        using (var streamWriter = new StreamWriter(await request.GetRequestStreamAsync()))
        {
            streamWriter.Write(itemToSend);
            streamWriter.Flush();
            streamWriter.Dispose();
        }

        // Send the request to the server and wait for the response:  
        using (var response = await request.GetResponseAsync())
        {
            // Get a stream representation of the HTTP web response:  
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                var message = reader.ReadToEnd();

                Debug.Log($"Sent transaction to server: {message}");
            }
        }
        
    }

}
